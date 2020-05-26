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
    }
}
