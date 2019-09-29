using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPL_Lib;

namespace Tpl
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                //Create the tpl process
                var tpl = new TplSearch(args[0]);

                var input = new List<TplResult>();
                string line;

                //Read input
                while ((line = Console.ReadLine()) != null)
                {
                    input.Add(new TplResult(line));
                }

                //Process
                var output = tpl.Process(input);

                foreach(var o in output)
                {
                    Console.WriteLine(o.PrintValuesOnly());
                }
            }
        }
    }
}
