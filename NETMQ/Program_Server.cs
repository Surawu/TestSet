using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace NETMQServer
{
    public partial class Program
    {
        internal static void Server_Rep(string port = "5554")
        {
            Console.WriteLine("Server started");
            using (var context = new ZContext())
            {
                using (var responder = new ZSocket(context, ZSocketType.REP))
                {
                    responder.Bind("tcp://127.0.0.1:" + port);
                    while (true)
                    {
                        var frame = responder.ReceiveMessage();
                        foreach (var item in frame)
                        {
                            var message = item.ReadString();
                            Console.WriteLine("get message: " + message);

                        }
                        responder.Send(new ZFrame("World " + port));
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        internal static void Server_Pub(string port = "5554")
        {
            Console.WriteLine("Server started");

            using (var context = new ZContext())
            {
                using (var responder = new ZSocket(context, ZSocketType.PUB))
                {
                    responder.Bind("tcp://127.0.0.1:" + port);
                    var msg = new ZMessage()
                    {
                        new ZFrame("hello1" + port),
                        new ZFrame("hello2" + port)
                    };
                    while (true)
                    {
                        responder.Send(msg);
                    }
                }
            }
        }

        public static void PipeLineVentilator()
        {
            using (var context = new ZContext())
            using (var ventilator = new ZSocket(context, ZSocketType.PUSH))
            {
                ventilator.Bind("tcp://127.0.0.1:5554");

                while (true)
                {
                    ventilator.Send(new ZFrame("Hello " + context.ContextPtr.ToInt32()));
                }
            }
        }
    }
}
