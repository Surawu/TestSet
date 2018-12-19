using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace UnLoadAssembly
{
    class Program
    {
        private const string dir = @"D:\VSProject\TestSet\UnLoadAssembly";
        static void Main(string[] args)
        {
            var files = Directory.GetFiles(Path.Combine(dir, "File"), "*.plc");
            var path = Path.Combine(dir, "Sura.dll");
            if (MD5Helper.CanForceCompile(files, path, out string hash))
            {
                MD5Helper.Compile(path, hash);
            }
        }
    }
}
