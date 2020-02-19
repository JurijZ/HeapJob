using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cwjobs
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new Engine();

            try
            {
                // Crawler
                engine.Execute_Crawler();

                // Matcher
                engine.Execute_Matcher();

                // Applier
                engine.Execute_Applier();
            }
            catch (Exception ex)
            {
                Logger.Log("EXCEPTION");
                Logger.Log(ex.Message);
                Logger.Log(ex.StackTrace);
            }            

            Logger.Log("Program execution successfully completed !!!");
        }
    }
}
