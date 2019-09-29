using TPL_Lib;
using TPL_Lib.Extensions;
using TPL_Lib.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using TPL_Lib.Tpl_Parser;

namespace TPL_Console_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            bool exit = false;

            while (!exit)
            {
                var input = File.ReadAllLines("in.txt").ToList();
                Console.Write("Search: ");
                var query = Console.ReadLine();

                var sw = new Stopwatch();
                sw.Restart();
                var search = new TplSearch(query);
                sw.Stop();
                var ConstructionTime = sw.ElapsedMilliseconds;

                sw.Restart();
                var results = search.Process(input.ToTplResults());
                sw.Stop();
                var ExecTime = sw.ElapsedMilliseconds;

                foreach (var r in results)
                {
                    Console.WriteLine(r);
                }

                Console.WriteLine("Query Construction took: " + ConstructionTime);
                Console.WriteLine("Execution took: " + ExecTime);

                Console.Write("Exit? (y/n): ");
                var yn = Console.ReadKey().KeyChar;
                Console.WriteLine();
                exit = yn == 'y';
            }
        }
    }
}
