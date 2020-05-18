using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TplLib.Functions.String_Functions;
using TplLib.Tpl_Parser;

namespace TplLib.Functions
{
    public abstract class TplFunction
    {
        #region Fields
        // REGEXs
        // Always pull parameters first
        //internal static readonly Regex _functionNameRegex = new Regex("^\\s*(?<funcName>[A-Za-z]+) *", RegexOptions.Compiled);
        //internal static readonly Regex _parameterRegex = new Regex("\\s*(?<paramName>[A-Za-z_]+) *= *(?<value>(([\"']).*?[\"'])|([A-Za-z0-9_]+))", RegexOptions.Compiled);
        //internal static readonly Regex _quotesRegex = new Regex("(?<!\\\\)([\"'])(?:(?=(\\\\?))\\2.)*?\\1", RegexOptions.Compiled);
        //After parameters are removed, field names can be extracted
        //protected static readonly Regex _fieldNameRegex =    new Regex(@"\s*(?<field>((?<=\$)[\w.]+|[_A-Za-z]+[A-Za-z0-9_.]*))(\s|,|$)+", RegexOptions.Compiled);
        //protected static readonly Regex _dirFieldNameRegex = new Regex(@"\s*(?<field>((?<=\$)[+-]?[\w.]+|[+-]?[_A-Za-z]+[A-Za-z0-9_.]*))[\s|,]*", RegexOptions.Compiled);

        //public static readonly Regex _nameAtEndRegex = new Regex(@"(^| +)([Aa]s|AS) +(?<Name>\w+)$", RegexOptions.Compiled);

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

        public List<TplResult> Process()
        {
            return Process(new List<TplResult>());
        }

        public List<TplResult> Process(IEnumerable<string> input)
        {
            return Process(input.Select(l => new TplResult(l)));
        }

        /// <summary>
        /// Runs the list of TplResults through all functions in the pipeline, and returns the output
        /// </summary>
        /// <param name="input">The list of rsults to process</param>
        /// <returns>A list of processed results</returns>
        public List<TplResult> Process(IEnumerable<TplResult> input)
        {
            var results = InnerProcess(input.ToList()); // Add to list so original collection is not modified

            if (NextFunction != null)
            {
                results = NextFunction.Process(results);
            }

            return results;
        }

        protected abstract List<TplResult> InnerProcess(List<TplResult> input);
    }
}
