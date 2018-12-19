using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UnLoadAssembly
{
    public class MD5Helper
    {
        public static string GetMd5HashWithFile(IEnumerable<string> list)
        {
            var sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.Append(File.OpenRead(item));
            }
            var md5Hasher = new MD5CryptoServiceProvider();
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(sb.ToString()));
            return BitConverter.ToString(data);
        }

        public static void Compile(string path, string hash)
        {
            var refAssems = new List<string>
            {
                "Microsoft.CSharp.dll",
                "System.Core.dll",
                "System.dll",
            };

            var resource = Path.Combine(Path.GetDirectoryName(path), "MyResources.resources");
            using (var writer = new ResourceWriter(resource))
            {
                writer.AddResource("MD5", hash);
            }

            CompilerParameters paras = new CompilerParameters(refAssems.ToArray(), path);
            paras.EmbeddedResources.Add(resource);
            CompilerResults results;
            using (CodeDomProvider provider = new CSharpCodeProvider())
            {
                results = provider.CompileAssemblyFromSource(paras, SourceCode);
                if (results.Errors.HasErrors)
                {
                    foreach (CompilerError item in results.Errors)
                    {
                        if (item.IsWarning)
                        {
                            continue;
                        }

                        Console.WriteLine(item.ErrorText);
                    }
                }
            }
        }

        public static bool CanForceCompile(IEnumerable<string> list, string path, out string hash)
        {
            hash = GetMd5HashWithFile(list);
            if (File.Exists(path) == false)
            {
                return true;
            }
            AppDomain tempDomain = null;
            var existHash = LoadAssembly(path, ref tempDomain);
            if (hash.Equals(existHash))
            {
                return false;
            }
            else
            {
                AppDomain.Unload(tempDomain);
                DeleteFile(path);
                return true;
            }
        }

        private static void DeleteFile(string path)
        {
            if (File.Exists(path) == false)
                return;

            var fi = new FileInfo(path);
            if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                fi.Attributes = FileAttributes.Normal;

            File.Delete(path);
        }

        private static string LoadAssembly(string path, ref AppDomain domain)
        {
            var domainInfo = new AppDomainSetup
            {
                ApplicationBase = Environment.CurrentDirectory
            };
            domain = AppDomain.CreateDomain("TempDomain", AppDomain.CurrentDomain.Evidence, domainInfo);
            Type type = typeof(Proxy);
            var instance = (Proxy)domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
            return instance.GetAssembly(path);
        }

        private static string SourceCode
        {
            get
            {
                return @"
using System;

namespace CodeDOMExample
{
    public class File
    {
        public File()
        {
            Name = ""Code complete 2"";
        }
        public string Name { get; set; }
    }
}
";
            }
        }
    }

    public class Proxy : MarshalByRefObject
    {
        public string GetAssembly(string path)
        {
            try
            {
                var assembly = Assembly.LoadFile(path);
                var mgr = new ResourceManager("MyResources", assembly);
                return mgr.GetString("MD5");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }
    }
}