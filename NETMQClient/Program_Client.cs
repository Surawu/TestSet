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
                    // .Subscribe("hello1")will WriteLine hello1 and hello2, but if you Subscribe("hello2")
                    // will not WriteLine any message
                    requester.SubscribeAll(); // this is very important

                    //requester.Connect("tcp://127.0.0.1:5555");
                    requester.Connect("tcp://127.0.0.1:5554");
                    while (true)
                    {
                        var reps = requester.ReceiveMessage();
                        foreach (var item in reps)
                        {
                            Console.WriteLine(item.ReadString());
                        }
                    }
                }
            }
        }

        public static void PipeLineSink()
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
    }
}
