using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using TplLib;

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
            //TplSearch tpl;

            //A query was provided directly on the command line
            //if (!string.IsNullOrWhiteSpace(options.TplQuery))
            //{
            //    tpl = new TplSearch(options.TplQuery);
            //}

            ////A query was provided via a Tpl file
            //else if (!string.IsNullOrWhiteSpace(options.TplFilePath))
            //{
            //    tpl = new TplSearch(System.IO.File.ReadAllText(options.TplFilePath));
            //}

            ////No query was provided
            //else
            //{
            //    //Throw an error?
            //    console.WriteLine("", ConsoleColor.DarkGray);
            //    return;
            //}


            //var input = new List<TplResult>();

            ////Set the TPL source if one was set explicitly by cmd line args
            //if (!string.IsNullOrWhiteSpace(options.InputFilePath))
            //{
            //    tpl.Source = options.InputFilePath;
            //}

            ////Read from Stdin
            //else if (options.ReadFromStdIn)
            //{
            //    string line;

            //    //Read input
            //    while ((line = Console.ReadLine()) != null)
            //    {
            //        input.Add(new TplResult(line));
            //    }
            //}

            //List<TplResult> results;

            ////No input, exit
            //if (input.Count == 0 && !tpl.HasSource)
            //{
            //    console.WriteLine("No input to process. Add a source parameter in your query, specify an input file (-i or -input) or pipe input into Tpl from another application (use -Stdin / -s switch)", ConsoleColor.Yellow);
            //    return;
            //}

            ////Process the input
            //else if (tpl.HasSource)
            //{
            //    results = tpl.Process();
            //}
            //else
            //{
            //    results = tpl.Process(input);
            //}

            ////Determine what to do with the output
            ////For not just print it all to the console
            //foreach (var o in results)
            //{
            //    console.WriteLine(o.PrintValuesOnly());
            //}
        }
    }
}
