using CommandLine;
using CommandLine.Text;
using Irony;
using System;
using System.Collections.Generic;
using TplLib.Functions;

namespace Tpl
{
    class Program
    {
        private static ConsoleColorWrapper console = new ConsoleColorWrapper();
        static void Main(string[] args)
        {
            try
            {
                //var parseSettings = new ParserSettings() { CaseSensitive = false };
                var results = new Parser(s => { s.CaseSensitive = false; s.EnableDashDash = false; })
                    .ParseArguments<Options>(args)
                    .WithParsed(options =>
                    {
                        ProcessOptions(options);
                    });

                if (args.Length == 0)
                {
                    var help = HelpText.AutoBuild(results, null, null);
                    help.Copyright = "Created by Will Brown";
                    console.WriteLine(help);
                }

                else
                    results.WithNotParsed(errors =>
                    {
                        console.WriteLine(HelpText.DefaultParsingErrorsHandler(results, new HelpText()), ConsoleColor.Red);
                    });
            }
            catch (Exception e)// when (e is ArgumentException)
            {
                console.WriteLine(e.Message, ConsoleColor.Red);
            }
        }

        private static void ProcessOptions(Options options)
        {
            TplFunction tpl;

            //A query was provided directly on the command line
            if (!string.IsNullOrWhiteSpace(options.TplQuery))
            {
                tpl = TplLib.Tpl.Create(options.TplQuery, out IReadOnlyList<LogMessage> errors);
            }

            //A query was provided via a Tpl file
            else if (!string.IsNullOrWhiteSpace(options.TplFilePath))
            {
                tpl = TplLib.Tpl.Create(System.IO.File.ReadAllText(options.TplFilePath), out IReadOnlyList<LogMessage> errors);
            }

            //No query was provided
            else
            {
                //Throw an error?
                console.WriteLine("", ConsoleColor.DarkGray);
                return;
            }

            var results = tpl.Process();

            //Determine what to do with the output
            //For not just print it all to the console
            foreach (var o in results)
            {
                console.WriteLine(o.PrintValuesOnly());
            }
        }
    }
}
