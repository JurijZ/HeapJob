using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace cwjobs
{
    public static class Logger
    {
        public static void Log(object message)
        {
            Console.WriteLine( message );

            //var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (StreamWriter sw = File.AppendText(DateTime.Now.ToString("yyyyMMdd") + "_log.txt"))
            {
                sw.WriteLine(DateTime.Now.ToUniversalTime() + " : " + message);
            }
        }
    }
}
