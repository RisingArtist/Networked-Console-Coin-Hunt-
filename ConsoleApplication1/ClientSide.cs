using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace ConsoleApplication1
{
    class ClientSide
    {
        //Client Stuff

        public static NetworkStream Connect()
        {
            Console.WriteLine("Client Started");
            DataMediator.clientSocket.Connect("127.0.0.1", 9001);
            Console.WriteLine("Client Socket Program - Server Connect ...");
            return DataMediator.clientSocket.GetStream();
        }

        public static void Close()
        {
            DataMediator.clientSocket.Close();
            System.Environment.Exit(0);
        }

        //Send the WASD to the server
        //In addition, the 'e' key to DISCONNECT
        public static void SendKey(string key)
        {
            NetworkStream serverStream = DataMediator.clientSocket.GetStream();

            //Send this message, with a $ at the end, to the server
            byte[] outstream = Encoding.ASCII.GetBytes(key.ToString() + "$");
            serverStream.Write(outstream, 0, outstream.Length);
            serverStream.Flush();
        }

        public static void SendInput()
        {
            //Getting the Keys in REAL-TIME
            if (Console.KeyAvailable)
            {
                //Sending the keys to the server
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.A:
                        ClientSide.SendKey("a");
                        break;
                    case ConsoleKey.S:
                        ClientSide.SendKey("s");
                        break;
                    case ConsoleKey.D:
                        ClientSide.SendKey("d");
                        break;
                    case ConsoleKey.W:
                        ClientSide.SendKey("w");
                        break;
                    case ConsoleKey.E:
                        ClientSide.SendKey("e");
                        ClientSide.Close();
                        break;
                    default:
                        Console.WriteLine("Not a WASD key! Try again!");
                        break;
                }
            }
        }

        public static string[] GetMessageFromServer(NetworkStream networkStream)
        {
            byte[] buffer = new byte[ClientSide.getClientSocket().ReceiveBufferSize];
            int byteRead = networkStream.Read(buffer, 0, buffer.Length);
            string messageFromServer = Encoding.ASCII.GetString(buffer, 0, byteRead);

            char[] delimited = new char[] { '?' };

            string[] parsed = messageFromServer.Split(delimited);
            return parsed;
        }

        // GETTER 
        public static TcpClient getClientSocket()
        {
            return DataMediator.clientSocket;
        }

    }
}
