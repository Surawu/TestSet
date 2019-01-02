using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NETMQServer
{
    public partial class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("please enter port...");
            ParallelTask(Console.ReadLine());
        }
    }
}
