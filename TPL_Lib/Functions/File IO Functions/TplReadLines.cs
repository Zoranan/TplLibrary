using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Functions.File_IO_Functions
{
    public class TplReadLines : TplFunction
    {
        private string _folderPath;
        private string _fileNamePattern;
        private string _fullPath;
        public string FilePath { get => _fullPath;
            set
            {
                _folderPath = Path.GetDirectoryName(value);
                _fileNamePattern = Path.GetFileName(value);
                _fullPath = value;
            }
        }
        public bool Recurse { get; internal set; }

        internal TplReadLines() { }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            if (!_fileNamePattern.Contains("*") && !Recurse)
                input.AddRange(
                    File.ReadAllLines(_fullPath)
                        .Select(l => new TplResult(l, _fullPath))
                    );

            else
                input.AddRange(
                    Directory.EnumerateFiles(_folderPath, _fileNamePattern, Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                        .SelectMany(path =>
                            File.ReadAllLines(path)
                                .Select(l => new TplResult(l, path)))
                    );

            return input;
        }
    }
}
