using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace NETMQWorker
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
    }
}
