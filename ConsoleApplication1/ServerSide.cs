using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace ConsoleApplication1
{
    class ServerSide
    {
        //The board
        public static string[,] board =
        {
            {"#", "#", "#", "#","#", "#", "#", "#", "#", "#" },
            {"#", " ", " ", " "," ", " ", " ", " ", "o", "#" },
            {"#", " ", "#", " "," ", " ", " ", " ", " ", "#" },
            {"#", "o", "#", " "," ", "#", " ", " ", " ", "#" },
            {"#", "#", "#", "#"," ", "#", " ", " ", " ", "#" },
            {"#", " ", " ", " "," ", "#", " ", " ", " ", "#" },
            {"#", " ", "#", "#"," ", "#", " ", " ", " ", "#" },
            {"#", " ", "o", "#"," ", " ", " ", " ", " ", "#" },
            {"#", " ", " ", "#"," ", " ", " ", " ", "o", "#" },
            {"#", "#", "#", "#","#", "#", "#", "#", "#", "#" },
        };

        static List<TcpClient> clientSockets = new List<TcpClient>(); //Lists of Clients 
        public static TcpListener serverSocket //Server is listening
        {
            get;
            private set;
        }

        // Initialize the server on Home; Port number, 9001, was just a random available pick made
        public static void InitializeServer()
        {
            string ipAddress = "127.0.0.1";
            serverSocket = new TcpListener(IPAddress.Parse(ipAddress), 9001);
            serverSocket.Start();
            Console.WriteLine(" >> Accept connection from client...");
        }

        // Receive incoming message and parse it
        public static string readMessageFromClient(NetworkStream networkStream)
        {
            byte[] bytesFrom = new byte[DataMediator.clientSocket.ReceiveBufferSize];
            networkStream.Read(bytesFrom, 0, DataMediator.clientSocket.ReceiveBufferSize);
            string dataFromClient = Encoding.ASCII.GetString(bytesFrom);
            return dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
        }

        // Check if player lands on coin
        public static bool isCoinSpot(int x, int y)
        {
            return (board[x, y] == "o");
        }

        //Check to see if the space is a 'wall' or player
        //If not, then you can move onto that spot. Otherwise, not allowed 
        public static bool ValidMove(int x, int y)
        {
            return (board[x, y] != "#" && board[x, y] != "P");
        }

        //Send the message back to the client. 
        //Contains client's ID, position, score, Just got coin?, and disconnect
        public static void sendMessageToClient(string portNum)
        {
            Console.WriteLine(DataMediator.clients[portNum] + "Position is: " + DataMediator.clients[portNum].posX.ToString() + ", " + DataMediator.clients[portNum].posY.ToString());
            foreach (TcpClient clientSocket in clientSockets)
            {
                if (clientSocket != null)
                {
                    NetworkStream networkStream = clientSocket.GetStream();
                    string serverResponse = portNum + "?" + DataMediator.clients[portNum].posX.ToString() + "?" + DataMediator.clients[portNum].posY.ToString() + "?" + DataMediator.clients[portNum].score + "?" + (DataMediator.clients[portNum].justGot ? "t" : "f") + "?" + (DataMediator.clients[portNum].disconnect ? "t" : "f");
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    //Flush 
                    networkStream.Flush();
                }

            }
        }

        public static void RequestMovement(TcpClient clientSocket, int portNum, string dataFromClient)
        {
            Console.WriteLine(" >> Data from client -" + dataFromClient);
            //If client is requesing movement, go thru here.
            if (dataFromClient == "w" || dataFromClient == "a" || dataFromClient == "s" ||
                dataFromClient == "d" || dataFromClient == "e") //This was on purpose for nostalgia reason.
            {
                int oldX = DataMediator.clients[portNum.ToString()].posX;
                int oldY = DataMediator.clients[portNum.ToString()].posY;

                switch (dataFromClient)
                {
                    case "w":
                        DataMediator.clients[portNum.ToString()].posX -= 1;
                        break;
                    case "s":
                        DataMediator.clients[portNum.ToString()].posX += 1;
                        break;
                    case "a":
                        DataMediator.clients[portNum.ToString()].posY -= 1;
                        break;
                    case "d":
                        DataMediator.clients[portNum.ToString()].posY += 1;
                        break;
                    case "e":
                        DataMediator.clients[portNum.ToString()].disconnect = true;
                        clientSocket.GetStream().Close();
                        clientSocket.Close();
                        getClientSockets()[getClientSockets().IndexOf(clientSocket)] = null;
                        break;
                    default:
                        Console.Write("Invalid Key (Server-Side)");
                        break;
                }

                DataMediator.clients[portNum.ToString()].justGot = false;
                if (!ValidMove(DataMediator.clients[portNum.ToString()].posX, DataMediator.clients[portNum.ToString()].posY))
                {
                    DataMediator.clients[portNum.ToString()].posX = oldX;
                    DataMediator.clients[portNum.ToString()].posY = oldY;
                }
                else if (isCoinSpot(DataMediator.clients[portNum.ToString()].posX, DataMediator.clients[portNum.ToString()].posY))
                {
                    DataMediator.clients[portNum.ToString()].score++;
                    DataMediator.clients[portNum.ToString()].justGot = true;
                    board[DataMediator.clients[portNum.ToString()].posX, DataMediator.clients[portNum.ToString()].posY] = " ";
                }


                sendMessageToClient(portNum.ToString());
                //Remove from the dictionary
                if (DataMediator.clients[portNum.ToString()].disconnect)
                {
                    DataMediator.clients.Remove(portNum.ToString());
                }
            }
        }

        public static void InitializeClient(int portNum, string dataFromClient)
        {
            //If the client request an ID, go thru here
            //Start-UP
            if (dataFromClient == "ID")
            {
                //Initializing the Client and Position
                if (!DataMediator.clients.ContainsKey(portNum.ToString()))
                {

                    Random rng = new Random();
                    int xCoord = rng.Next(1, 10);
                    int yCoord = rng.Next(1, 10);
                    //Check to see if space is valid
                    while (!ValidMove(xCoord, yCoord) || (isCoinSpot(xCoord, yCoord)))
                    {
                        xCoord = rng.Next(1, 10);
                        yCoord = rng.Next(1, 10);
                    }
                    DataMediator.clients.Add(portNum.ToString(), new ClientInfo(xCoord, yCoord, 0));
                }
                sendMessageToClient(portNum.ToString());
            }
        }

        //Display the Board into the Console.
        //In addition, it will display the client(s) info
        public static void DisplayBoard()
        {

            string[,] board_copy = board.Clone() as string[,];

            Console.Clear();
            foreach (KeyValuePair<string, ClientInfo> client in DataMediator.clients)
            {
                if (!client.Value.disconnect)
                {
                    board_copy[client.Value.posX, client.Value.posY] = "P";
                }
                if (client.Value.justGot)
                {
                    board[client.Value.posX, client.Value.posY] = " ";
                }
            }
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    Console.Write(board_copy[x, y]);
                }
                Console.WriteLine("");
            }
            foreach (KeyValuePair<string, ClientInfo> client in DataMediator.clients)
            {
                Console.WriteLine("Client #" + client.Key + "Position: " + client.Value.posX + ", " + client.Value.posY);
                Console.WriteLine("Score: " + client.Value.score);
                Console.WriteLine("Got it: " + client.Value.justGot);

                Console.WriteLine("Clients' Key:");
                Console.WriteLine(DataMediator.clients.Count());
                Console.WriteLine(client.Key);
                Console.WriteLine("");
            }
        }

        public static List<TcpClient> getClientSockets()
        {
            return clientSockets;
        }
    }
}
