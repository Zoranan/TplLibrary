using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TplLib.Tpl_Parser;

namespace TplLib.Functions
{
    public class TplGroup : TplFunction
    {
        public Regex StartRegex { get; internal set; }
        public Regex EndRegex { get; internal set; }
        public string TargetField { get; internal set; }

        internal TplGroup() { }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            var results = new List<TplResult>();
            var sb = new StringBuilder();
            var grouping = false;

            foreach (var i in input)
            {
                if (i.HasField(TargetField))
                {
                    var currentValue = i.StringValueOf(TargetField);

                    //Start our group
                    if (!grouping && StartRegex.IsMatch(currentValue))
                    {
                        grouping = true;
                    }

                    //Add to our group
                    if (grouping)
                    {
                        sb.AppendLine(currentValue);
                    }

                    //End our group
                    if (grouping && EndRegex.IsMatch(currentValue))
                    {
                        i.AddOrUpdateField(TargetField, sb.ToString().TrimEnd());
                        results.Add(i);

                        grouping = false;
                        sb.Clear();
                    }
                }
            }

            return results;
        }
    }
}
