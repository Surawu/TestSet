using System;
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
    }
}
