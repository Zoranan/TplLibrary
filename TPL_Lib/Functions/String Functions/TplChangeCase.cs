using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TplLib.Extensions;
using TplLib.Tpl_Parser;

namespace TplLib.Functions.String_Functions
{
    public class TplChangeCase : TplFunction
    {
        public List<string> TargetFields { get; internal set; }
        public string TargetGroup { get; internal set; } = null;
        public Regex MatchingRegex { get; internal set; } = null;
        public bool ToUpper { get; set; } = false;

        internal TplChangeCase() { }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            //No matching regex was specified, so match the whole thing

            if (MatchingRegex == null)
            {
                Parallel.ForEach(input, result =>
                {
                    foreach (var field in TargetFields)
                    {
                        result.AddOrUpdateField(field, ToUpper ? result.StringValueOf(field).ToUpper() : result.StringValueOf(field).ToLower());
                    }
                });
            }

            else
            {
                Parallel.ForEach(input, result =>
                {
                    foreach (var field in TargetFields)
                    {
                        var fValue = result.StringValueOf(field);
                        var matches = MatchingRegex.Matches(fValue);
                        StringBuilder sb = new StringBuilder();

                        //Add the initial bit of our string if needed
                        if (matches.Count > 0 && matches[0].Index > 0)
                        {
                            if (TargetGroup == null)
                                sb.Append(fValue.Substring(0, matches[0].Index));

                            else
                                sb.Append(fValue.Substring(0, matches[0].Groups[TargetGroup].Index));
                        }

                        foreach (Match m in matches)
                        {
                            string val;

                            if (TargetGroup == null)
                                val = m.Value;

                            else if (TargetGroup != null && m.Groups[TargetGroup].Success)
                                val = m.Groups[TargetGroup].Value;

                            else
                                continue;

                            if (TargetGroup == null)
                                sb.Append(fValue.Substring(sb.Length, m.Index - sb.Length));

                            else
                                sb.Append(fValue.Substring(sb.Length, m.Groups[TargetGroup].Index - sb.Length));

                            sb.Append(ToUpper ? val.ToUpper() : val.ToLower());

                        }

                        //Add the remaining bit of our string if needed
                        if (sb.Length < fValue.Length)
                            sb.Append(fValue.Substring(sb.Length));

                        //Store it
                        result.AddOrUpdateField(field, sb.ToString());
                    }
                });
            }

            return input;
        }
    }
}
