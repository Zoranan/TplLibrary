using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TPL_Lib.Functions.String_Functions;
using TPL_Lib.Tpl_Parser;

namespace TPL_Lib.Functions
{
    public abstract class TplFunction
    {
        #region Fields
        // REGEXs
        // Always pull parameters first
        internal static readonly Regex _functionNameRegex = new Regex("^\\s*(?<funcName>[A-Za-z]+) *", RegexOptions.Compiled);
        internal static readonly Regex _parameterRegex = new Regex("\\s*(?<paramName>[A-Za-z_]+) *= *(?<value>(([\"']).*?[\"'])|([A-Za-z0-9_]+))", RegexOptions.Compiled);
        internal static readonly Regex _quotesRegex = new Regex("(?<!\\\\)([\"'])(?:(?=(\\\\?))\\2.)*?\\1", RegexOptions.Compiled);
        //After parameters are removed, field names can be extracted
        protected static readonly Regex _fieldNameRegex =    new Regex(@"\s*(?<field>((?<=\$)[\w.]+|[_A-Za-z]+[A-Za-z0-9_.]*))(\s|,|$)+", RegexOptions.Compiled);
        protected static readonly Regex _dirFieldNameRegex = new Regex(@"\s*(?<field>((?<=\$)[+-]?[\w.]+|[+-]?[_A-Za-z]+[A-Za-z0-9_.]*))[\s|,]*", RegexOptions.Compiled);

        public static readonly Regex _nameAtEndRegex = new Regex(@"(^| +)([Aa]s|AS) +(?<Name>\w+)$", RegexOptions.Compiled);

        public TplFunction NextFunction { get; protected set; } = null;

        /// <summary>
        /// Adds the next function to the very end of this pipeline
        /// </summary>
        /// <param name="next">The function to attach to the end of the pipeline</param>
        public void AddToPipeline(TplFunction next)
        {
            
            if (NextFunction == null)
            {
                NextFunction = next;
            }
            else
            {
                NextFunction.AddToPipeline(next);
            }
        }
        #endregion

        /// <summary>
        /// Runs the list of TplResults through all functions in the pipeline, and returns the output
        /// </summary>
        /// <param name="input">The list of rsults to process</param>
        /// <returns>A list of processed results</returns>
        public List<TplResult> Process(List<TplResult> input = null)
        {
            var results = InnerProcess(input);

            if (NextFunction != null)
            {
                results = NextFunction.Process(results);
            }

            return results;
        }

        #region Compilation

        //protected string GetUnescapedQuoteValue(string inQuote)
        //{
        //    var quoteMatch = _quotesRegex.Match(inQuote);
        //    if (quoteMatch.Success)
        //    {
        //        char quoteType = quoteMatch.Value[0];
        //        var quotedText = quoteMatch.Value.Substring(1, quoteMatch.Length - 2);
        //        quotedText = quotedText.Replace(@"\" + quoteType, quoteType.ToString());
        //        //quotedText = quotedText.Replace("\\'", "'");
        //        return quotedText;
        //    }
        //    else
        //    {
        //        throw new ArgumentException("An error occured while parsing a quoted value: " + inQuote);
        //    }
        //}

        //protected List<string> ParseQuotes(ref string quotesString)
        //{
        //    var quoteMatches = _quotesRegex.Matches(quotesString);
        //    var quotes = new List<string>(quoteMatches.Count);

        //    Match previous = null;
        //    var queryAsChars = quotesString.ToCharArray();

        //    foreach (Match fm in quoteMatches)
        //    {
        //        quotes.Add(GetUnescapedQuoteValue(fm.Value));

        //        //Remove the current match from the char array
        //        for (int i = fm.Index; i < fm.Index + fm.Length; i++)
        //        {
        //            queryAsChars[i] = ' ';
        //        }

        //        //Replace commas between quotes
        //        if (previous != null)
        //        {
        //            var startI = previous.Index + previous.Length;
        //            var endI = fm.Index;

        //            for (int i = startI; i < endI; i++)
        //            {
        //                if (queryAsChars[i] == ',')
        //                    queryAsChars[i] = ' ';
        //            }
        //        }

        //        previous = fm;
        //    }

        //    quotesString = new string(queryAsChars).TrimStart(new char[] { ' ', ',' });
        //    return quotes;
        //}

        //protected string ParseAssignmentFieldName(ref string fieldsString, Regex argReg = null)
        //{
        //    if (argReg == null)
        //        argReg = _nameAtEndRegex;

        //    var fieldMatch = argReg.Match(fieldsString);

        //    if (fieldMatch.Success)
        //    {
        //        fieldsString = fieldsString.Substring(0, fieldMatch.Index);
        //        return fieldMatch.Groups["Name"].Value;
        //    }
        //    else
        //        return null;
        //}

        //protected List<string> ParseFieldNames(ref string fieldsString, Regex argReg = null)
        //{
        //    if (argReg == null)
        //        argReg = _dirFieldNameRegex;

        //    var fieldMatches = argReg.Matches(fieldsString);
        //    var fields = new List<string>(fieldMatches.Count);

        //    foreach (Match fm in fieldMatches)
        //    {
        //        fields.Add(fm.Groups["field"].Value);
        //        fieldsString = fieldsString.Replace(fm.Value, "");
        //    }

        //    return fields;
        //}

        //protected Dictionary<string, string> ParseParameters(ref string query)
        //{
        //    var _params = new Dictionary<string, string>();

        //    var matches = _parameterRegex.Matches(query);

        //    if (matches.Count > 0)
        //    {
        //        var queryAsChars = query.ToCharArray();
        //        Match previous = null;
        //        foreach (Match m in matches)
        //        {
        //            string paramVal;

        //            try
        //            {
        //                paramVal = GetUnescapedQuoteValue(m.Groups["value"].Value);
        //            }
        //            catch
        //            {
        //                paramVal = m.Groups["value"].Value;
        //            }

        //            _params.Add(m.Groups["paramName"].Value.ToLower(), paramVal);

        //            //Comma validation and removal
        //            if (previous != null)
        //            {
        //                var startI = previous.Index + previous.Length;
        //                var endI = m.Index;

        //                for (int i = startI; i<endI; i++)
        //                {
        //                    if (queryAsChars[i] == ',')
        //                        queryAsChars[i] = ' ';
        //                }
        //            }

        //            previous = m;
        //        }

        //        query = _parameterRegex.Replace(new string(queryAsChars), "").TrimStart(new char[] { ' ', ',' });
        //    }

        //    return _params;
        //}

        //protected List<string> ParseExtraBits(ref string queryString)
        //{
        //    var output = queryString.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        //    queryString = "";
        //    return output;
        //}

        //protected bool ValidateQueryProcessed(string query)
        //{
        //    if (query.Trim().Length > 0)
        //    {
        //        throw new ArgumentException("Invalid token \"" + query.Trim() + "\"");
        //    }

        //    return true;
        //}

        #endregion

        //Get a Tpl function from a query string

        internal TplFunction GetTplFunctionFromQuery(string query)
        {
            TplFunction func;

            var funcString = new ParsableString(query);
            var fName = funcString.GetNext(TokenType.WORD);

            if (!fName.IsSuccess)
                throw new InvalidOperationException($"Invalid function name token");

            switch (fName.OriginalValue.ToLower())//TODO: Should this be case insensitive?
            {
                case "dedup":
                    func = new TplDedup(funcString);
                    break;
                case "rex":
                    func = new TplRegex(funcString);
                    break;
                case "select":
                    func = new TplSelect(funcString);
                    break;
                case "sort":
                    func = new TplSort(funcString);
                    break;
                case "keyvalue":
                case "kv":
                    func = new TplKeyValue(funcString);
                    break;
                case "stats":
                    func = new TplStats(funcString);
                    break;
                case "where":
                    func = new TplWhere(funcString);
                    break;
                case "eval":
                    func = new TplEval(funcString);
                    break;
                case "replace":
                    func = new TplReplace(funcString);
                    break;
                case "pad":
                    func = new TplStringPad(funcString);
                    break;
                case "substring":
                    func = new TplSubstring(funcString);
                    break;
                case "concat":
                    func = new TplStringConcat(funcString);
                    break;
                case "between":
                    func = new TplBetween(funcString);
                    break;
                case "tolower":
                    func = new TplChangeCase(funcString);
                    break;
                case "toupper":
                    func = new TplChangeCase(funcString);
                    ((TplChangeCase)func).ToUpper = true;
                    break;
                case "group":
                    func = new TplGroup(funcString);
                    break;
                default:
                    throw new InvalidOperationException($"{fName.OriginalValue} is not a recognised function name");

            }
            if (func != null)
                return func;

            else
                throw new InvalidOperationException(string.Format("\"{0}\" is not a recognised command", fName));
        }

        protected abstract List<TplResult> InnerProcess(List<TplResult> input);
    }
}
