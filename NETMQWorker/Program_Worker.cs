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
        public static void Worker_Pull_Push()
        {
            using (var context = new ZContext())
            using (ZSocket receiver = new ZSocket(context, ZSocketType.PULL),
                sender = new ZSocket(context, ZSocketType.PUSH))
            {
                receiver.Connect("tcp://127.0.0.1:5554");
                sender.Connect("tcp://127.0.0.1:5555");

                while (true)
                {
                    foreach (var item in receiver.ReceiveMessage())
                    {
                        Console.WriteLine(item.ReadString());
                    }

                    var msg = new ZMessage()
                    {
                        new ZFrame("worker "+DateTime.Now.ToShortTimeString()),
                    };
                    sender.Send(msg);
                    Thread.Sleep(50);
                }
            }
        }
    }
}
