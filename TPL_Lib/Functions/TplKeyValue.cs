using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TPL_Lib.Extensions;
using TPL_Lib.Tpl_Parser;

namespace TPL_Lib.Functions
{
    // PIPELINE SYNTAX
    //
    // keyvalue 
    // keyvalue fieldName1, fieldName2
    // keyvalue kvDelim=":"
    // keyvalue fieldName1, fieldName2 kvDelim=":"
    // kv ...
    //
    // Accepted kvDelim values: (:/\|=, and space)
    //

    /// <summary>
    /// Automatically searches for key-value pairs in each TplResult and adds them as fields
    /// Use the 'kvDelim=":::"' option to specify the key value deliminator (default is "=")
    /// Specify one or more field names to search through by entering their names
    /// </summary>
    public class TplKeyValue : TplFunction
    {
        #region Regex's
        protected Regex _keyValueRegex = null;
        #endregion

        #region Properties
        public List<string> TargetFields { get; private set; }
        public Regex KeyValueRegex
        {
            get { return _keyValueRegex; }
            private set
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
        public TplKeyValue (string kvRex, List<string> targetFields = null)
        {
            KeyValueRegex = new Regex(kvRex, RegexOptions.Compiled);

            if (targetFields == null)
                TargetFields = new string[] { TplResult.DEFAULT_FIELD }.ToList();
        }

        public TplKeyValue (ParsableString query)
        {
            query.GetNextList(TokenType.VAR_NAME)
                .OnSuccess(fields => TargetFields = fields.ResultsList.Select(f => f.Value()).ToList())
                .OnFailure(_ => TargetFields = new List<string>() { TplResult.DEFAULT_FIELD })

                .GetNext(TokenType.QUOTE)
                .OnSuccess(kvRegex => KeyValueRegex = new Regex(kvRegex.Value(), RegexOptions.Compiled))
                .OnFailure(_ => throw new ArgumentException($"The keyvalue function requires a regex as its final argument, in quotes"))
                
                .Source.VerifyAtEnd();
        }

        #endregion

        #region Processing
        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            foreach (var r in input)
            {
                foreach (var field in TargetFields)
                {
                    var matches = _keyValueRegex.Matches(r.ValueOf(field));

                    foreach (Match m in matches)
                    {
                        r.AddOrUpdateField(m.Groups["key"].Value, m.Groups["value"].Value);
                    }
                }
            }

            return input;
        }
        #endregion
    }
}
