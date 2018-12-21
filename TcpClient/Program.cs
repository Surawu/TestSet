using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyTcpClient
{
    class Program
    {
        static Socket ClientSocket;
        static void Main(string[] args)
        {
            Client client = new Client();
            client.Transfer();
            Console.ReadLine();
        }
    }
}
