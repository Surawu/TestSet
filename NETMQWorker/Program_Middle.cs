using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace NETMQMiddle
{
    public static partial class Program
    {
        internal static void PipeLineBroker()
        {
            using (var context = new ZContext())
            using (var pullSocket = new ZSocket(context, ZSocketType.PULL))
            using (var pushSocket = new ZSocket(context, ZSocketType.PUSH))
            {
                pullSocket.Connect("tcp://127.0.0.1:5554");
                pushSocket.Connect("tcp://127.0.0.1:5555");

                while (true)
                {
                    var pullMsg = pullSocket.ReceiveMessage();
                    var m = pullMsg[0].ReadString();
                    Console.WriteLine(m);

                    Console.WriteLine("send " + m);
                    pushSocket.Send(new ZFrame(m));

                    Thread.Sleep(1000);
                }

            }
        }

        internal static void MServerMode()
        {
            using (var context = new ZContext())
            using (var router = new ZSocket(context, ZSocketType.ROUTER))
            using (var dealer = new ZSocket(context, ZSocketType.DEALER))
            {
                router.Bind("tcp://127.0.0.1:5554");
                dealer.Bind("tcp://127.0.0.1:5555");

                ZContext.Proxy(router, dealer);
            }
        }

        internal static void MTRelay()
        {
            using (var ctx = new ZContext())
            using (var receiver = new ZSocket(ctx, ZSocketType.PAIR))
            {
                //ConcurrentDictionary
                receiver.Bind("inproc://step3");
                Task.Factory.StartNew(() =>
                {
                    MTRelat_Step2(ctx);
                });
                receiver.ReceiveFrame();
                Console.WriteLine("Test successful!");
            }
        }

        static void MTRelat_Step2(ZContext ctx)
        {
            using (var receiver = new ZSocket(ctx, ZSocketType.PAIR))
            {
                receiver.Bind("inproc://step2");
                Task.Factory.StartNew(() =>
                {
                    MTRelat_Step1(ctx);
                });
                receiver.ReceiveFrame();
            }

            using (var sign = new ZSocket(ctx, ZSocketType.PAIR))
            {
                sign.Connect("inproc://step3");
                Console.WriteLine("Step 2 ready, signaling step 3");
                sign.Send(new ZFrame("Ready"));
            }
        }

        static void MTRelat_Step1(ZContext ctx)
        {
            using (var sign = new ZSocket(ctx, ZSocketType.PAIR))
            {
                sign.Connect("inproc://step2");
                Console.WriteLine("Step1 ready, siganl step 2");
                sign.Send(new ZFrame("Ready"));
            }
        }
    }
}
