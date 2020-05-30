using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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

                if (string.IsNullOrWhiteSpace(_folderPath))
                    _folderPath = @".\";
            }
        }
        public bool Recurse { get; internal set; }

        internal TplReadLines() { }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            var dict = new ConcurrentDictionary<string, ConcurrentDictionary<long, TplResult>>();
            var filePaths = Directory.EnumerateFiles(_folderPath, _fileNamePattern, Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            Parallel.ForEach(filePaths, filePath =>
            {
                var fileDict = new ConcurrentDictionary<long, TplResult>();

                Parallel.ForEach(File.ReadLines(filePath), (line, _, lineNum) =>
                {
                    var attempts = 0;
                    while (!fileDict.TryAdd(lineNum, new TplResult(line, filePath)))
                    {
                        if (attempts > 5)
                            throw new FileLoadException($"Error adding line {lineNum} from file {filePath}");

                        attempts++;
                    };
                });

                while (!dict.TryAdd(filePath, fileDict)) ;  //Add the file
            });

            //Combine all results from each file in proper order
            foreach (var filePath in filePaths)
            {
                var fileDict = dict[filePath];

                for (long i = 0; i < fileDict.Count; i++)
                    input.Add(fileDict[i]);
            }

            return input;
        }
    }
}
