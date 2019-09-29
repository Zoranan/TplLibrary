using TPL_Lib.Extensions;
using TPL_Lib.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using TPL_Lib.Functions.String_Functions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using TPL_Lib.Tpl_Parser;

namespace TPL_Lib
{

    /// <summary>
    /// The TplSearch class is what handles the entire TplQuery, and creates the pipeline from it.
    /// </summary>
    public class TplSearch : TplFunction, INotifyPropertyChanged
    {

        #region Enums
        public enum SearchMode { SEARCH, REGEX };
        public enum State { Created, Compiling, Loading, Processing, Done };
        #endregion

        #region Static Regex's
        public static readonly Regex _singleLineCommentsRegex = new Regex(@"\/\/.*?$", RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex _querySplitRegex = new Regex(@"\s*\|\s*", RegexOptions.Compiled);
        //private static readonly Regex _searchQueryRegex = new Regex("^\\s*([\"'])(?<search>.*?)\\1", RegexOptions.Compiled);
        internal static readonly Regex _escapedWildCardRegex = new Regex(@"(?<!\\)\\\*", RegexOptions.Compiled);
        private static readonly Regex _fileDirAndPathRegex = new Regex(@"^(?<dir>.*?)(?<recursiveFlag>\\\*)?\\(?<file>[^\\]+)$", RegexOptions.Compiled);
        #endregion

        #region INotify
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Fields
        private Regex _regex;
        private bool _hasSourceParam = false;
        private string _srcPath;
        private string _srcDirectory;
        private string _srcFilePattern;
        SearchOption _searchOption = SearchOption.TopDirectoryOnly;
        State _status = State.Created;
        private int _percentComplete = 0;
        #endregion

        #region Properties
        public SearchMode Mode { get; private set; } = SearchMode.SEARCH;
        public State Status
        {
            get { return _status; }
            private set
            {
                _status = value;
                NotifyPropertyChanged();
            }
        }
        public string Source
        {
            get { return _srcPath; }
            set
            {
                if (value == null)
                {
                    _srcPath = null;
                    _srcDirectory = null;
                    _srcFilePattern = null;
                }
                else
                {
                    var match = _fileDirAndPathRegex.Match(value);

                    if (match.Success)
                    {
                        _srcPath = value;
                        _hasSourceParam = true;
                        _srcDirectory = match.Groups["dir"].Value;
                        _srcFilePattern = match.Groups["file"].Value;

                        if (match.Groups["recursiveFlag"].Success)
                        {
                            _searchOption = SearchOption.AllDirectories;
                        }
                    }

                    else throw new ArgumentException("Unable to parse file path: " + value);
                }

            }
        }
        public string Query { get; set; }
        public int Percent
        {
            get { return _percentComplete; }
            set
            {
                if (_percentComplete >= 0 && _percentComplete <= 100)
                    _percentComplete = value;

                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Constructors
        public TplSearch ()
        {
            
        }

        //Function that copies the values of the input search object, and connects its downstream pipeline functions to itself
        private void CopyFrom (TplSearch search)
        {
            _srcDirectory = search._srcDirectory;
            _srcFilePattern = search._srcFilePattern;
            _hasSourceParam = search._hasSourceParam;

            Mode = search.Mode;
            Status = search.Status;
            Query = search.Query;
            Percent = search.Percent;

            AddToPipeline(search.NextFunction);
        }

        public TplSearch (string query)
        {
            Status = State.Compiling;

            #region Remove Comments First

            //var lines = Regex.Split(query, @"\r\n|\r|\n");
            //StringBuilder queryRecon = new StringBuilder();

            //foreach (var l in lines)
            //{
            //    var comment = _singleLineCommentsRegex.Match(l);
            //    bool remove = comment.Success;

            //    if (comment.Success)
            //    {
            //        var commentQuotes = _quotesRegex.Matches(l);

            //        //Check to see if the comment is between quotes
            //        foreach (Match q in commentQuotes)
            //        {
            //            if (q.Success && q.Index < comment.Index && q.Index + q.Length > comment.Index)
            //            {
            //                remove = false;
            //                break;
            //            }
            //        }
            //    }

            //    if (remove)
            //        queryRecon.AppendLine(l.Substring(0, comment.Index));
            //    else
            //        queryRecon.AppendLine(l);
            //}

            //query = queryRecon.ToString().Trim();

            //#endregion

            //#region Splitting the query safely
            ////We have to replace all | that we want to keep with another 
            ////character so the split doesnt throw them away
            //char[] queryChars = query.ToCharArray();
            //var quoteMatches = _quotesRegex.Matches(query);

            //foreach (Match match in quoteMatches)
            //{
            //    for (int i = match.Index; i < match.Index + match.Length; i++)
            //    {
            //        if (queryChars[i] == '|')
            //        {
            //            queryChars[i] = '║';
            //        }
            //    }
            //}

            //query = new string(queryChars);
            //var pieces = _querySplitRegex.Split(query);

            ////Now put the pipes back
            //for (int j=0; j < pieces.Length; j++)
            //{
            //    pieces[j] = pieces[j].Replace('║', '|');
            //}
            //#endregion

            ////Attempt to get a function from the first piece, before processing it as a search
            //bool processAsSearch = false;
            //try
            //{
            //    AddToPipeline(GetTplFunctionFromQuery(pieces[0]));
            //}
            //catch (ArgumentException)
            //{
            //    processAsSearch = true;
            //}

            ////If the first piece did not contain a valid function, attempt to process it as a search
            //if (processAsSearch)
            //{
            //    #region Parameter handling
            //    //Check for parameters in the search first
            //    var parameters = ParseParameters(ref pieces[0]);
            //    if (parameters.Count > 0)
            //    {
            //        foreach (var p in parameters)
            //        {
            //            switch (p.Key)
            //            {
            //                case "mode":
            //                    switch (p.Value)
            //                    {
            //                        case "search":
            //                            Mode = SearchMode.SEARCH;
            //                            break;
            //                        case "rex":
            //                            Mode = SearchMode.REGEX;
            //                            break;
            //                        default:
            //                            throw new ArgumentException(p.Key + " only accepts values of \"search\" or \"rex\"");
            //                    }
            //                    break;

            //                case "subfolders":
            //                    switch (p.Value)
            //                    {
            //                        case "true":
            //                            _searchOption = SearchOption.AllDirectories;
            //                            break;

            //                        case "false":
            //                            _searchOption = SearchOption.TopDirectoryOnly;
            //                            break;

            //                        default:
            //                            throw new ArgumentException(p.Key + " only accepts values of \"true\" or \"false\"");
            //                    }
            //                    break;

            //                case "source":

            //                    var match = _fileDirAndPathRegex.Match(p.Value);

            //                    if (match.Success)
            //                    {
            //                        _hasSourceParam = true;
            //                        _srcDirectory = match.Groups["dir"].Value;
            //                        _srcFilePattern = match.Groups["file"].Value;

            //                        if (match.Groups["recursiveFlag"].Success)
            //                        {
            //                            _searchOption = SearchOption.AllDirectories;
            //                        }
            //                    }

            //                    else throw new ArgumentException("Unable to parse file path: " + p.Value);
            //                    break;

            //                default:
            //                    throw new ArgumentException("Unknown parameter: " + p.Key);
            //            }
            //        }
            //    }

            //    #endregion Parameter handling

            //    #region Initial search query processing
            //    //Getting the search from the beginning of the query
            //    var quotes = ParseQuotes(ref pieces[0]);

            //    if (quotes.Count == 0 && Mode == SearchMode.SEARCH)
            //    {
            //        Query = "";
            //        _regex = null;
            //    }
            //    else if (quotes.Count == 0)
            //    {
            //        throw new ArgumentException("You must specify a regex, enclosed in quotes");
            //    }
            //    else if (quotes.Count == 1)
            //    {
            //        Query = quotes[0];

            //        if (Mode == SearchMode.SEARCH)
            //        {
            //            Query = _escapedWildCardRegex.Replace("^" + Regex.Escape(Query) + "$", ".*?");
            //            _regex = new Regex(Query, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            //        }

            //        else
            //        {
            //            _regex = new Regex(Query, RegexOptions.Compiled);
            //        }
            //    }
            //    else if (quotes.Count > 1 && Mode == SearchMode.REGEX)
            //    {
            //        throw new ArgumentException("You may only specify one search term when using 'rex' mode in a search");
            //    }

            //    //Convert multiple search quotes in normal search mode, into a regex
            //    else
            //    {
            //        StringBuilder sb = new StringBuilder();

            //        sb.Append("^(");
            //        sb.Append(_escapedWildCardRegex.Replace(Regex.Escape(quotes[0]), ".*?"));

            //        foreach (var q in quotes.Skip(1))
            //        {
            //            sb.Append("|" + _escapedWildCardRegex.Replace(Regex.Escape(q), ".*?"));
            //        }

            //        sb.Append(")$");

            //        Query = sb.ToString();
            //        _regex = new Regex(Query, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            //    }

            //    ValidateQueryProcessed(pieces[0]);

            //    #endregion Initial search query processing
            //}

            //#region Downstream pipeline element creation

            ////Create the other pipeline elements
            //foreach (var p in pieces.Skip(1))
            //{
            //    AddToPipeline(GetTplFunctionFromQuery(p));
            //}

            #endregion

            //Split the full query up
            var queryFunctions = query.SplitUnquoted();
            bool processAsSearch = false;

            //Try to add the first item. If it fails, we need to attempt to process it as a search
            try { AddToPipeline(GetTplFunctionFromQuery(queryFunctions[0])); }
            catch (InvalidOperationException){ processAsSearch = true; }

            if (processAsSearch)
            {
                var searchString = new ParsableString(queryFunctions[0]);
                string regexString = null;

                searchString.GetNext(TokenType.QUOTE)
                    .OnSuccess(quote => regexString = quote.OriginalValue)
                    
                    .WhileGetNext(TokenType.PARAMETER, param =>
                    {
                        var pName = param.ParamName.Value().ToLower();
                        var pValue = param.ParamValue.Value().ToLower();

                        if (pName == "mode")
                        {
                            if (pValue == "search")
                                Mode = SearchMode.SEARCH;
                            else if (pValue == "rex")
                                Mode = SearchMode.REGEX;
                            else throw new ArgumentException($"{param.ParamName} only accepts values of \"search\" or \"rex\"");
                        }
                        else if (pName == "recurse")
                        {
                            _searchOption = pValue.ValueAs<bool>() ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
                        }
                        else if (pName == "source")
                        {
                            var match = _fileDirAndPathRegex.Match(pValue);

                            if (match.Success)
                            {
                                _hasSourceParam = true;
                                _srcDirectory = match.Groups["dir"].Value;
                                _srcFilePattern = match.Groups["file"].Value;

                                if (match.Groups["recursiveFlag"].Success)
                                    _searchOption = SearchOption.AllDirectories;
                            }

                            else throw new ArgumentException($"Unable to parse file path: '{pValue}'");
                        }
                    })

                    .Source.VerifyAtEnd();

                if (regexString == null && Mode == SearchMode.REGEX)
                    throw new ArgumentException($"No search regex was specified");

                else if (regexString != null && Mode == SearchMode.SEARCH)
                    _regex = new Regex($"^{Regex.Escape(regexString).Replace(@"\*", ".*")}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                else if (regexString != null)
                    _regex = new Regex(regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }


            foreach (var funcStr in queryFunctions.Skip(1))
                AddToPipeline(GetTplFunctionFromQuery(funcStr));

            Status = State.Done;
        }

        #endregion Constructor

        #region Loading Files

        private List<TplResult> LoadFromFiles()
        {
            var results = new List<TplResult>();

            try
            {
                Status = State.Loading;
                var fNames = Directory.GetFiles(_srcDirectory, _srcFilePattern, _searchOption);

                //Get total size of all files
                double totalSizeOfAllFiles = 0;
                var sizes = new Dictionary<string, long>();
                foreach (var f in fNames)
                {
                    var size = new FileInfo(f).Length;
                    totalSizeOfAllFiles += size;
                    sizes.Add(f, size);
                }

                int currentTotalPercent = 0;

                //Begin loading each file
                foreach (var f in fNames)
                {
                    int currentFilePercent = 0;
                    double currentPosition = 0;

                    //Read each line
                    using (var tr = File.OpenText(f))
                    {
                        string l;
                        while ((l = tr.ReadLine()) != null)
                        {
                            //Create a result from that line
                            results.Add(new TplResult(l, f));

                            //Calculate percentages
                            currentPosition += l.Length + 2;
                            currentFilePercent = (int)(currentPosition / sizes[f] * 100);
                            Percent = currentTotalPercent + (int)(sizes[f] / totalSizeOfAllFiles * currentFilePercent);
                        }

                        currentTotalPercent = Percent;
                    }

                }

                //Push percent to 100 at the end
                Percent = 100;
                Status = State.Done;
            }
            catch(Exception e) { Console.WriteLine(e.Message); Console.WriteLine(e.StackTrace); }

            return results;
        }

        #endregion

        #region Processing
        protected override List<TplResult> InnerProcess(List<TplResult> input = null)
        {
            if (input == null)
                if (_hasSourceParam)
                    input = LoadFromFiles();
                else
                    throw new DirectoryNotFoundException("No input was provided for the query to process. Try specifying a source path, or passing a result list into the query");

            //Get the inital results
            Status = State.Processing;
            List<TplResult> results;

            if (_regex != null)
                results = new TplRegex(_regex).Process(input);

            else
                results = input;

            Status = State.Done;

            //Return
            return results;
        }
        #endregion
    }
}
