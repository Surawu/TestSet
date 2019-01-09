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

        internal static void Server_Pub()
        {
            Console.WriteLine("Server started");

            using (var context = new ZContext())
            {
                using (var publisher = new ZSocket(context, ZSocketType.PUB))
                {
                    publisher.Bind("tcp://127.0.0.1:5554");
                    while (true)
                    {
                        Console.WriteLine("Sending Hello....");
                        publisher.Send(new ZFrame("Hello"));
                    }
                }
            }
        }

        internal static void PipeLineVentilator()
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

        internal static void MServerMode()
        {
            using (var context = new ZContext())
            using (var respondent = new ZSocket(context, ZSocketType.REP))
            {
                respondent.Connect("tcp://127.0.0.1:5555");

                while (true)
                {
                    var msg = respondent.ReceiveMessage()[0].ReadString();
                    Console.WriteLine(msg);

                    respondent.Send(new ZFrame("Hello world"));
                }
            }
        }

        const int SyncPub_SubscribersExpected = 3;	// We wait for 3 subscribers

        internal static void SyncPub()
        {
            using (var context = new ZContext())
            using (var pubSocket = new ZSocket(context, ZSocketType.PUB))
            using (var repSocket = new ZSocket(context, ZSocketType.REP))
            {
                repSocket.Bind("tcp://127.0.0.1:5554");
                pubSocket.Bind("tcp://127.0.0.1:5555");
                //pubSocket.SendHighWatermark = 1100000; what's this??

                int subscribers = SyncPub_SubscribersExpected;

                do
                {
                    Console.WriteLine("Waiting for {0} subscriber" + (subscribers > 1 ? "s" : string.Empty) + "...", subscribers);
                    repSocket.ReceiveFrame();
                    repSocket.Send(new ZFrame());
                } while (--subscribers > 0);

                for (int i = 0; i < 20; i++)
                {
                    Console.WriteLine("Sending {0}...", i);
                    pubSocket.Send(new ZFrame(i));
                }
                pubSocket.Send(new ZFrame("End"));

            }
        }

        internal static void MTServer()
        {
            int a = 0;
            using (var context = new ZContext())
            using (var clients = new ZSocket(context, ZSocketType.ROUTER))
            using (var workers = new ZSocket(context, ZSocketType.DEALER))
            {
                clients.Bind("tcp://127.0.0.1:5554");
                workers.Bind("inproc://worker");

                for (int i = 0; i < 5; i++)
                {
                    Task.Factory.StartNew((ctx) =>
                    {
                        using (var server = new ZSocket(context, ZSocketType.REP))
                        {
                            server.Connect("inproc://worker");
                            using (var frame = server.ReceiveFrame())
                            {
                                var n = frame.ReadInt32();
                                Console.Write("Received: {0}", n);
                                // Do some 'work'
                                //Thread.Sleep(1);
                                a += n;

                                // Send reply back to client
                                Console.WriteLine("Sending {0}...", a);
                                server.Send(new ZFrame(a));
                            }
                        }
                    }, context);
                }

                ZContext.Proxy(clients, workers);
            }
        }
    }
}
