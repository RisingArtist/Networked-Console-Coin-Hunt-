using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace ConsoleApplication1
{
    static class DataMediator
    {
        public static TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        public static Dictionary<string, ClientInfo> clients = new Dictionary<string, ClientInfo>(); //Each Client has its unique info
    }
}
