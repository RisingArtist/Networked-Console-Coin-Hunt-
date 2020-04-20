using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;


namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            // Decide if this instance of the program should be the Server or Client
            string choice;
            while (true)
            {
                Console.WriteLine("0 - Start a Server!");
                Console.WriteLine("1 - Start a Client!");
                choice = Console.ReadLine();
                if (choice == "0" || choice == "1")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid Choice!");
                }
            }

            if (choice == "0")
            {
                //Server Code
                #region SERVER_CODE
                //Create the server
                ServerSide.InitializeServer();
                
                // Game loop
                while (true)
                {
                    try
                    {
                        //Checks to see if Client is Connecting
                        if (ServerSide.serverSocket.Pending())
                        {
                            ServerSide.getClientSockets().Add(ServerSide.serverSocket.AcceptTcpClient());
                        }
                        //Checking with every client
                        foreach (TcpClient clientSocket in ServerSide.getClientSockets())
                        {

                            if (clientSocket != null)
                            {
                                int portNum = ((IPEndPoint)clientSocket.Client.RemoteEndPoint).Port;
                                NetworkStream networkStream = clientSocket.GetStream();
                                //Checks if this client did send something
                                if (networkStream.DataAvailable)
                                {
                                    // Read 
                                    string dataFromClient = ServerSide.readMessageFromClient(networkStream);

                                    // Handle Request
                                    ServerSide.RequestMovement(clientSocket, portNum, dataFromClient);

                                    // Create new client
                                    ServerSide.InitializeClient(portNum, dataFromClient);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                #endregion
            }
            else
            {
                //Client Code
                #region CLIENT_CODE

                NetworkStream networkStream = ClientSide.Connect();

                //Get unique ID from server
                ClientSide.SendKey("ID");

                // Game loop
                while (true)
                {
                    ClientSide.SendInput();
                    //Reading from the server
                    if (networkStream.DataAvailable)
                    {
                        string[] parsed = ClientSide.GetMessageFromServer(networkStream);

                        //Parsing the Server message
                        string clientId = parsed[0];
                        string clientPosX = parsed[1];
                        string clientPosY = parsed[2];
                        string clientScore = parsed[3];
                        string clientGot = parsed[4];
                        string clientDisconnect = parsed[5];

                        //If client doesn't exist, create one in the dictionary,
                        //Otherwise, just update the existed client
                        if (!DataMediator.clients.ContainsKey(clientId))
                        {
                            DataMediator.clients.Add(clientId, new ClientInfo(Convert.ToInt32(clientPosX), Convert.ToInt32(clientPosY), Convert.ToInt32(clientScore)));
                        }
                        else
                        {
                            DataMediator.clients[clientId].posX = Convert.ToInt32(clientPosX);
                            DataMediator.clients[clientId].posY = Convert.ToInt32(clientPosY);
                            DataMediator.clients[clientId].score = Convert.ToInt32(clientScore);
                            DataMediator.clients[clientId].justGot = (clientGot == "t");
                            DataMediator.clients[clientId].disconnect = (clientDisconnect == "t");
                        }

                        if (DataMediator.clients[clientId].disconnect)
                        {
                            DataMediator.clients.Remove(clientId);
                        }

                        //Display the Board on the console
                        ServerSide.DisplayBoard();
                    }
                }
                #endregion
            }
        }
    }
}
