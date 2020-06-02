using TplLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TplLib.Tpl_Parser;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace TplLib.Functions
{
    /// <summary>
    /// Applys a specified regex to a list of TplResults, storing named capture groups as fields in each TplResult
    /// </summary>
    public class TplRegex : TplFunction
    {
        #region Properties
        public Regex Rex { get; internal set; }
        public string TargetField { get; internal set; } = TplResult.DEFAULT_FIELD;
        public bool PassThru { get; internal set; } = false;
        #endregion

        #region Constructors
        internal TplRegex() { }

        public TplRegex(Regex r, bool passthru = false, string targetField = TplResult.DEFAULT_FIELD)
        {
            Rex = r;
            TargetField = targetField;
            PassThru = passthru;
        }
        #endregion

        #region Processing
        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            var dict = new ConcurrentDictionary<long, TplResult>();
            long resultNum = 0;
            Parallel.ForEach(input, result =>
            {
                if (result.HasField(TargetField))
                {
                    var match = Rex.Match(result.StringValueOf(TargetField));

                    if (match.Success)
                    {
                        var groupNames = Rex.GetNamedCaptureGroupNames();
                        foreach (var key in groupNames)
                        {
                            result.AddOrUpdateField(key, match.Groups[key].Value);
                        }

                        if (!PassThru)
                        {
                            dict.TryAdd(resultNum, result);
                            resultNum++;
                        }
                    }
                }
            });

            //Return
            if (!PassThru)
            {
                var output = new List<TplResult>(dict.Count);

                for (long i = 0; i < dict.Count; i++)
                    output.Add(dict[i]);

                return output;
            }
            else
                return input;
        }
        #endregion
    }
}
