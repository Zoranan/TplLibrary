using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpl
{
    public class Options
    {
        private string _tplFilePath = null;
        [Option('f', "File", Required = false, HelpText = "Path to the TPL query file")]
        public string TplFilePath { get => _tplFilePath; 
            set
            {
                if (_tplQuery == null)
                    _tplFilePath = value;
                else
                    throw new ArgumentException("The --File / -f option can not be used at the same time as the --Tpl / -t option", nameof(TplFilePath));

                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("The --File / -f option can not be empty");
            }
        }

        private string _tplQuery = null;
        [Option('t', "Tpl", Required = false, HelpText = "The TPL query to use on the input")]
        public string TplQuery { get => _tplQuery; 
            set
            {
                if (_tplFilePath == null)
                    _tplQuery = value;
                else
                    throw new ArgumentException("The --Tpl / -t option can not be used at the same time as the --File / -f option", nameof(TplQuery));

                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("The --Tpl / -t option can not be empty");
            }
        }

        private string _inputFile = null;
        [Option('i', "InFile", Required = false, HelpText = "Path to the input file to be processed")]
        public string InputFilePath { get => _inputFile;
            set
            {
                if (!ReadFromStdIn)
                    _inputFile = value;
                else
                    throw new ArgumentException("You can not use the --InFile / -i option at the same time as the --Stdin / -s option", nameof(InputFilePath));
            }
        }

        private bool _readFromStdIn = false;
        [Option('s', "Stdin", Required = false, HelpText = "Specifies that the Tpl should read input from the pipeline", Default = false)]
        public bool ReadFromStdIn { get => _readFromStdIn;
            set
            {
                if (InputFilePath == null)
                    _readFromStdIn = value;
                else if (_readFromStdIn)
                    throw new ArgumentException("You can not use the --Stdin / -s option at the same time as the --InFile / -i option", nameof(InputFilePath));
            }
        }
    }
}
