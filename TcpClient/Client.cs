using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyTcpClient
{
    public class Client
    {
        private Socket clientSocket;

        public Client()
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8885);
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(endPoint);
            Console.WriteLine(clientSocket.Handle.ToInt32());
        }

        public void Transfer()
        {
            Console.WriteLine("开始发送消息");
            clientSocket.Send(Encoding.ASCII.GetBytes("Connect the Server"));
            var bytes = new byte[1024];
            if (clientSocket.Receive(bytes) > 0)
            {
                Console.WriteLine("Receive: " + Encoding.ASCII.GetString(bytes));
            }
            //注意此处close了话，就得重新开启服务器进行再次连接 -- wuzhaoyu 2018.12.21
            //clientSocket.Close();
        }
    }
}
