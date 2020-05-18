using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using TplLib;
using TplLib.Functions;

namespace TplConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            bool exit = false;

            while (!exit)
            {
                var input = File.ReadAllLines("in.txt").ToList();
                //Console.Write("Search: ");
                var query = Console.ReadLine();
                //TplFunction tpl = Tpl.Create(@"replace '' '' -as -new");
                //try
                //{
                //    tpl = Tpl.FromQuery(query);
                //    var results = tpl.Process(input.Select(i => new TplResult(i, "in.txt")).ToList());

                //    foreach (var r in results)
                //        Console.WriteLine(r.PrintValuesOnly());
                //}
                //catch (AggregateException e)
                //{
                //    Console.WriteLine(e.Message);

                //    foreach (var ex in e.InnerExceptions)
                //        Console.WriteLine("\t" + e.Message);
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine(e.Message);
                //}


                //foreach (var r in results)
                //{
                //    Console.WriteLine(r);
                //}

                //Console.WriteLine("Query Construction took: " + ConstructionTime);
                //Console.WriteLine("Execution took: " + ExecTime);

                Console.Write("Exit? (y/n): ");
                var yn = Console.ReadKey().KeyChar;
                Console.WriteLine();
                exit = yn == 'y';
            }
        }
    }
}
