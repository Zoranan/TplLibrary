using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TPL_Lib.Tpl_Parser;

namespace TPL_Lib.Functions
{
    public class TplGroup : TplFunction
    {
        public Regex StartRegex { get; private set; } = null;
        public Regex EndRegex { get; private set; } = null;
        public string TargetField { get; private set; } = null;

        public TplGroup(ParsableString query)
        {
            query.GetNext(TokenType.VAR_NAME)
                .OnSuccess(field => TargetField = field.Value())
                .OnFailure(_ => TargetField = TplResult.DEFAULT_FIELD)

                .GetNext(TokenType.QUOTE)
                .OnFailure(_ => throw new ArgumentException($"Missing start regex in group function"))
                .OnSuccess(start => StartRegex = new Regex(start.Value(), RegexOptions.Compiled))

                .GetNext(TokenType.QUOTE)
                .OnFailure(_ => throw new ArgumentException($"Missing end regex in group function"))
                .OnSuccess(end => EndRegex = new Regex(end.Value(), RegexOptions.Compiled))

                .Source.VerifyAtEnd();
        }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            var results = new List<TplResult>();
            var sb = new StringBuilder();
            var grouping = false;

            foreach (var i in input)
            {
                if (i.HasField(TargetField))
                {
                    var currentValue = i.ValueOf(TargetField);

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
