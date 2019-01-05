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
        public static void TaskWork()
        {
            //
            // Task worker
            // Connects PULL socket to tcp://127.0.0.1:5557
            // Collects workloads from ventilator via that socket
            // Connects PUSH socket to tcp://127.0.0.1:5558
            // Sends results to sink via that socket
            //
            // Author: metadings
            //

            // Socket to receive messages on and
            // Socket to send messages to
            using (var context = new ZContext())
            using (var receiver = new ZSocket(context, ZSocketType.PULL))
            using (var sink = new ZSocket(context, ZSocketType.PUSH))
            {
                receiver.Connect("tcp://127.0.0.1:5557");
                sink.Connect("tcp://127.0.0.1:5558");

                // Process tasks forever
                while (true)
                {
                    var replyBytes = new byte[4];
                    receiver.ReceiveBytes(replyBytes, 0, replyBytes.Length);
                    int workload = BitConverter.ToInt32(replyBytes, 0);
                    Console.WriteLine("{0}.", workload);    // Show progress

                    Thread.Sleep(workload); // Do the work

                    sink.Send(new byte[0], 0, 0);   // Send results to sink
                }
            }
        }

        internal static void RRBroker()
        {
            using (var context = new ZContext())
            using (var frontend = new ZSocket(context, ZSocketType.ROUTER))
            using (var backend = new ZSocket(context, ZSocketType.DEALER))
            {
                frontend.Bind("tcp://127.0.0.1:5554");
                backend.Bind("tcp://127.0.0.1:5555");
                ZContext.Proxy(frontend, backend);
            }
        }
    }
}
