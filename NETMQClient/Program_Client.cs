using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace NETMQClient
{
    public static partial class Program
    {
        internal static void Client_Req()
        {
            using (var context = new ZContext())
            {
                using (var requester = new ZSocket(context, ZSocketType.REQ))
                {
                    requester.Connect("tcp://127.0.0.1:5554");
                    var msg = new ZMessage()
                    {
                        new ZFrame("hello1"),
                        new ZFrame("hello2")
                    };
                    while (true)
                    {
                        requester.Send(msg);
                        var reps = requester.ReceiveMessage();
                        foreach (var item in reps)
                        {
                            Console.WriteLine(item.ReadString());
                        }
                    }
                }
            }
        }

        internal static void Client_Sub()
        {
            using (var context = new ZContext())
            {
                using (var requester = new ZSocket(context, ZSocketType.SUB))
                {
                    requester.Subscribe("Hello"); // this is very important

                    requester.Connect("tcp://127.0.0.1:5554");
                    while (true)
                    {
                        var msg = requester.ReceiveFrame();
                        Console.WriteLine("Received: " + msg.ReadString());
                    }
                }
            }
        }

        internal static void PipeLineSink()
        {
            using (var context = new ZContext())
            using (var sink = new ZSocket(context, ZSocketType.PULL))
            {
                sink.Bind("tcp://127.0.0.1:5555");

                while (true)
                {
                    Console.WriteLine(sink.ReceiveMessage()[0]);
                }
            }
        }

        internal static void MClientMode()
        {
            bool flag = false;
            using (var context = new ZContext())
            using (var requester = new ZSocket(context, ZSocketType.REQ))
            {
                requester.Connect("tcp://127.0.0.1:5554");

                while (true)
                {
                    flag = !flag;
                    requester.Send(new ZFrame("Hello " + flag));

                    var msg = requester.ReceiveMessage();
                    Console.WriteLine(msg[0].ReadString());
                }
            }
        }

        internal static void SyncSub()
        {
            using (var context = new ZContext())
            using (var subSocket = new ZSocket(context, ZSocketType.SUB))
            using (var reqSocket = new ZSocket(context, ZSocketType.REQ))
            {
                subSocket.Connect("tcp://127.0.0.1:5555");
                subSocket.SubscribeAll();
                reqSocket.Connect("tcp://127.0.0.1:5554");
                reqSocket.Send(new ZFrame());
                reqSocket.ReceiveFrame();

                int i = 0;
                while (true)
                {
                    using (var frame = subSocket.ReceiveFrame())
                    {
                        var str = frame.ReadString();
                        if (str.Equals("End"))
                        {
                            break;
                        }

                        Console.WriteLine("Receiving {0}....", frame.ReadInt32());
                        i++;
                    }
                }

                Console.WriteLine("Receiving {0} messages", i.ToString());
            }
        }

        internal static void MTClient()
        {
            using (var context = new ZContext())
            using (var req = new ZSocket(context, ZSocketType.REQ))
            {
                req.Connect("tcp://127.0.0.1:5554");

                while (true)
                {
                    req.SendFrame(new ZFrame(10));

                    using (var frame = req.ReceiveFrame())
                    {
                        Console.WriteLine(frame.ReadInt32());
                    }
                }
            }
        }
    }
}
