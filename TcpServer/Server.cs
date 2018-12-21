using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace TcpServer
{
    public class Server
    {
        Action<Socket> action;
        public Server()
        {
            var ip = IPAddress.Parse("127.0.0.1");
            var port = 8885;
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(ip, port));
            server.Listen(10);
            Console.WriteLine("建立连接");
            var receive = new byte[2048];
            //Socket transferSocket = connectSocket.Accept();
            //transferSocket.Receive(receive);
            Console.WriteLine("server " + server.Handle.ToInt32());

            action += RecceiveData;
            server.BeginAccept(AsyncCallback, server);
        }

        // 远程主机即服务器
        private void AsyncCallback(IAsyncResult ar)
        {
            Console.WriteLine("Client connected");
            var server = ((Socket)ar.AsyncState);
            Socket transfer = server.EndAccept(ar);
            // 仅用于接收传入的连接请求
            server.BeginAccept(AsyncCallback, server);
            action(transfer);
        }

        public void RecceiveData(Socket socket)
        {
            while (socket.Connected)
            {
                var bytes = new Byte[1024];
                if (socket.Receive(bytes) > 0)
                {
                    Console.WriteLine(Encoding.ASCII.GetString(bytes));
                    socket.Send(Encoding.ASCII.GetBytes("Send back message"));
                }
            }
        }
    }
}
