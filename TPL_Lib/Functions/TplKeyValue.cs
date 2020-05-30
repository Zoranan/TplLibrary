using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TplLib.Extensions;
using TplLib.Tpl_Parser;

namespace TplLib.Functions
{
    /// <summary>
    /// Searches for key-value pairs using a supplied regex in each TplResult and adds them as fields
    /// </summary>
    public class TplKeyValue : TplFunction
    {
        #region Regex's
        protected Regex _keyValueRegex = null;
        #endregion

        #region Properties
        public List<string> TargetFields { get; internal set; }
        public Regex KeyValueRegex
        {
            get { return _keyValueRegex; }
            internal set
            {
                var groups = value.GetNamedCaptureGroupNames();
                if (groups.Contains("key") && groups.Contains("value"))
                    _keyValueRegex = value;
                else
                    throw new ArgumentException("Invalid KeyValue regex! The regex must contain named capture groups 'key' and 'value'");
            }
        }
        #endregion

        #region Constructors
        internal TplKeyValue() { }

        public TplKeyValue (string kvRex, List<string> targetFields = null)
        {
            KeyValueRegex = new Regex(kvRex, RegexOptions.Compiled);

            if (targetFields == null)
                TargetFields = new string[] { TplResult.DEFAULT_FIELD }.ToList();
        }

        //public TplKeyValue (ParsableString query)
        //{
        //    query.GetNextList(TokenType.VAR_NAME)
        //        .OnSuccess(fields => TargetFields = fields.ResultsList.Select(f => f.Value()).ToList())
        //        .OnFailure(_ => TargetFields = new List<string>() { TplResult.DEFAULT_FIELD })

        //        .GetNext(TokenType.QUOTE)
        //        .OnSuccess(kvRegex => KeyValueRegex = new Regex(kvRegex.Value(), RegexOptions.Compiled))
        //        .OnFailure(_ => throw new ArgumentException($"The keyvalue function requires a regex as its final argument, in quotes"))
                
        //        .Source.VerifyAtEnd();
        //}

        #endregion

        #region Processing
        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            Parallel.ForEach(input, result =>
            {
                foreach (var field in TargetFields)
                {
                    var matches = _keyValueRegex.Matches(result.StringValueOf(field));

                    foreach (Match m in matches)
                    {
                        result.AddOrUpdateField(m.Groups["key"].Value, m.Groups["value"].Value);
                    }
                }
            });

            return input;
        }
        #endregion
    }
}
