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
                using (var requester = new ZSocket(context, ZSocketType.SUB | ZSocketType.id))
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

        // Pipeline Pattern (PUSH/PULL)
        internal static void Client_Pull()
        {
            using (var context = new ZContext())
            using (var socket = new ZSocket(context, ZSocketType.PULL)) // 连接至任务分发器
            {
                socket.Bind("tcp://127.0.0.1:5555");
                while (true)
                {
                    foreach (var item in socket.ReceiveMessage())
                    {
                        Console.WriteLine(item.ReadString());
                    }
                }
            }
        }

        internal static void Client_NOBLOCK() // TODO:混用套接字类型
        {
            using (var context = new ZContext())
            using (var receiver = new ZSocket(context, ZSocketType.PULL)) // 连接至任务分发器
            using (var subscriber = new ZSocket(context, ZSocketType.SUB)) // Connect to server
            {
                receiver.Connect("tcp://127.0.0.1:5554");
                subscriber.Connect("tcp://127.0.0.1:5555");
                subscriber.SetOption(ZSocketOption.SUBSCRIBE, "10001");

                while (true)
                {
                    foreach (var item in subscriber.ReceiveMessage())
                    {
                        Console.WriteLine(item.ReadString());
                    }
                    foreach (var item in receiver.ReceiveMessage())
                    {
                        Console.WriteLine(item.ReadString());
                    }
                }
            }
        }

        internal static void MSPoller()
        {
            using (var context = new ZContext())
            using (var receiver = new ZSocket(context, ZSocketType.PULL))
            using (var subscriber = new ZSocket(context, ZSocketType.SUB))
            {
                receiver.Connect("tcp://127.0.0.1:5554");
                subscriber.Connect("tcp://127.0.0.1:5555");

                subscriber.SetOption(ZSocketOption.SUBSCRIBE, "10001");
                var sockets = new ZSocket[] { subscriber, receiver };
                var polls = new ZPollItem[] { ZPollItem.CreateReceiver(), ZPollItem.CreateReceiver() };
                ZError error;
                ZMessage[] message;
                while (true)
                {
                    if (sockets.PollIn(polls, out message, out error))
                    {
                        if (message[0] != null)
                        {
                            foreach (var item in message[0])
                            {
                                Console.WriteLine(item.ReadString());
                            }
                        }
                        if (message[1] != null)
                        {
                            foreach (var item in message[1])
                            {
                                Console.WriteLine(item.ReadString());
                            }
                        }
                    }
                    else
                    {
                        if (error == ZError.ETERM)
                            return; // Interrupted
                        if (error != ZError.EAGAIN)
                            throw new ZException(error);
                    }
                }

            }
        }

        internal static void TaskSink()
        {
            //
            // Task sink
            // Binds PULL socket to tcp://127.0.0.1:5558
            // Collects results from workers via that socket
            //
            // Author: metadings
            //

            // Prepare our context and socket
            using (var context = new ZContext())
            using (var sink = new ZSocket(context, ZSocketType.PULL))
            {
                sink.Bind("tcp://*:5558");

                // Wait for start of batch
                sink.ReceiveFrame();

                // Start our clock now
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                // Process 100 confirmations
                for (int i = 0; i < 100; ++i)
                {
                    sink.ReceiveFrame();

                    if ((i / 10) * 10 == i)
                        Console.Write(":");
                    else
                        Console.Write(".");
                }

                // Calculate and report duration of batch
                stopwatch.Stop();
                Console.WriteLine("Total elapsed time: {0} ms", stopwatch.ElapsedMilliseconds);
            }
        }

        internal static void RRClient()
        {
            using (var context = new ZContext())
            using (var requester = new ZSocket(context, ZSocketType.REQ))
            {
                requester.Connect("tcp://127.0.0.1:5554");
                var msg = new ZMessage()
                    {
                        new ZFrame("hello "+ AppDomain.GetCurrentThreadId().ToString() )
                    };
                while (true)
                {
                    requester.Send(msg);

                    foreach (var item in requester.ReceiveMessage())
                    {
                        Console.WriteLine(item.ReadString());
                    }
                }
            }
        }

        internal static void MTClient()
        {
            using (var context = new ZContext())
            using (var requester = new ZSocket(context, ZSocketType.REQ))
            {
                requester.Connect("tcp://127.0.0.1:5554");
            }
        }

        public static void MTRelay()
        {
            //
            // Multithreaded relay
            //
            // Author: metadings
            //

            // Bind inproc socket before starting step2
            using (var ctx = new ZContext())
            using (var receiver = new ZSocket(ctx, ZSocketType.PAIR))
            {
                receiver.Bind("inproc://step3");

                new Thread(() => MTRelay_step2(ctx)).Start();

                // Wait for signal
                receiver.ReceiveFrame();

                Console.WriteLine("Test successful!");
            }
        }

        private static void MTRelay_step2(ZContext ctx)
        {
            // Bind inproc socket before starting step1
            using (var receiver = new ZSocket(ctx, ZSocketType.PAIR))
            {
                receiver.Bind("inproc://step2");

                new Thread(() => MTRelay_step1(ctx)).Start();

                // Wait for signal and pass it on
                receiver.ReceiveFrame();
            }

            // Connect to step3 and tell it we're ready
            using (var xmitter = new ZSocket(ctx, ZSocketType.PAIR))
            {
                xmitter.Connect("inproc://step3");

                Console.WriteLine("Step 2 ready, signaling step 3");
                xmitter.Send(new ZFrame("READY"));
            }
        }

        static void MTRelay_step1(ZContext ctx)
        {
            // Connect to step2 and tell it we're ready
            using (var xmitter = new ZSocket(ctx, ZSocketType.PAIR))
            {
                xmitter.Connect("inproc://step2");
                Console.WriteLine("Step 1 ready, signaling step 2");
                xmitter.Send(new ZFrame("READY"));
            }
        }


        public static void PathoSub()
        {
            //
            // Pathological subscriber
            // Subscribes to one random topic and prints received messages
            //
            // Author: metadings
            //

            using (var context = new ZContext())
            using (var subscriber = new ZSocket(context, ZSocketType.SUB))
            {
                subscriber.Connect("tcp://127.0.0.1:5556");

                var rnd = new Random();
                var subscription = string.Format("{0:D3}", rnd.Next(1000));
                subscriber.Subscribe(subscription);
                //subscriber.Subscribe("Sura"); 三帧消息

                ZMessage msg;
                ZError error;
                while (true)
                {
                    if (null == (msg = subscriber.ReceiveMessage(out error)))
                    {
                        if (error == ZError.ETERM)
                            break;  // Interrupted
                        throw new ZException(error);
                    }
                    using (msg)
                    {
                        if (msg[0].ReadString() != subscription)
                        {
                            throw new InvalidOperationException();
                        }
                        Console.WriteLine(msg[1].ReadString());
                    }
                }
            }
        }
    }
}
