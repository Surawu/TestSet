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
        /// <summary>
        /// 任务执行器
        /// 连接PULL套接字至tcp://localhost:5554端点
        /// 从任务分发器处获取任务
        /// 连接PUSH套接字至tcp://localhost:5555端点
        /// 向结果采集器发送结果
        /// </summary>
        public static void ParallelTask()
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
