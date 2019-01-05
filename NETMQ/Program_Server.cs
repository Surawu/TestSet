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

        internal static void Push_Pub()
        {
            Console.WriteLine("Server started");
            using (var context = new ZContext())
            using (var pub = new ZSocket(context, ZSocketType.PUB))
            using (var push = new ZSocket(context, ZSocketType.PUSH))
            {
                pub.Bind("tcp://127.0.0.1:5555");
                push.Bind("tcp://127.0.0.1:5554");
                var msg = new ZMessage()
                {
                    new ZFrame("10001 hello1 5554")
                };
                var msg1 = new ZMessage()
                {
                    new ZFrame("hello1 5555")
                };
                for (int i = 0; i < 100; i++)
                {
                    pub.Send(msg);
                    push.Send(msg1);
                    Thread.Sleep(50);
                }
            }
        }

        internal static void TaskVent()
        {
            //
            // Task ventilator
            // Binds PUSH socket to tcp://127.0.0.1:5557
            // Sends batch of tasks to workers via that socket
            //
            // Author: metadings
            //

            // Socket to send messages on and
            // Socket to send start of batch message on
            using (var context = new ZContext())
            using (var sender = new ZSocket(context, ZSocketType.PUSH))
            using (var sink = new ZSocket(context, ZSocketType.PUSH))
            {
                sender.Bind("tcp://*:5557");
                sink.Connect("tcp://127.0.0.1:5558");

                Console.WriteLine("Press ENTER when the workers are ready...");
                Console.ReadKey(true);
                Console.WriteLine("Sending tasks to workers...");

                // The first message is "0" and signals start of batch
                sink.Send(new byte[] { 0x00 }, 0, 1);

                // Initialize random number generator
                var rnd = new Random();

                // Send 100 tasks
                int i = 0;
                long total_msec = 0;    // Total expected cost in msecs
                for (; i < 100; ++i)
                {
                    // Random workload from 1 to 100msecs
                    int workload = rnd.Next(100) + 1;
                    total_msec += workload;
                    byte[] action = BitConverter.GetBytes(workload);

                    Console.WriteLine("{0}", workload);
                    sender.Send(action, 0, action.Length);
                }

                Console.WriteLine("Total expected cost: {0} ms", total_msec);
            }
        }

        internal static void RRServer()
        {
            using (var context = new ZContext())
            using (var respond = new ZSocket(context, ZSocketType.REP))
            {
                respond.Connect("tcp://127.0.0.1:5555");
                var msg = new ZMessage()
                    {
                        new ZFrame("world ")
                    };
                while (true)
                {
                    foreach (var item in respond.ReceiveMessage())
                    {
                        Console.WriteLine(item.ReadString());
                    }

                    respond.Send(msg);
                    Thread.Sleep(100);
                }
            }
        }

        internal static void MTServer()
        {
            using (var context = new ZContext())
            using (var clients = new ZSocket(context, ZSocketType.ROUTER)) // 用于和client进行通信的套接字
            using (var workers = new ZSocket(context, ZSocketType.DEALER)) // 用于和worker进行通信的套接字
            {
                clients.Bind("tcp://127.0.0.1:5554");
                workers.Bind("inproc://workers");

                for (int i = 0; i < 5; i++)
                {
                    Task.Factory.StartNew(() => { MTServer_Worker(context); });
                }

                ZContext.Proxy(clients, workers);
            }
        }

        static void MTServer_Worker(ZContext context)
        {
            using (var server = new ZSocket(context, ZSocketType.REP))
            {
                server.Connect("inproc://workers");
                while (true)
                {
                    var frame = server.ReceiveFrame();
                    Console.Write("Received: {0}", frame.ReadString());

                    Thread.Sleep(1);
                    string replyText = "World";
                    Console.WriteLine(", Sending: {0}", replyText);
                    server.Send(new ZFrame(replyText));
                }
            }
        }
    }
}
