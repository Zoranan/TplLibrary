using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TplLib.Tpl_Parser;

namespace TplLib.Functions
{
    public class TplBetween : TplFunction
    {
        public string TargetField { get; internal set; }
        public string StartValue { get; internal set; }
        public string EndValue { get; internal set; }
        public bool Inclusive { get; internal set; } = false;

        internal TplBetween() { }

        public TplBetween(string targetField, string startVal, string endVal, bool inclusive = false)
        {
            TargetField = targetField;
            StartValue = startVal;
            EndValue = endVal;
            Inclusive = inclusive;
        }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            var results = new List<TplResult>();
            int startIndex = -1;

            for (int i = 0; i < input.Count; i++)
            {
                var val = input[i].StringValueOf(TargetField);

                if (val.Contains(StartValue))
                    startIndex = i;

                if (startIndex != -1 && val.Contains(EndValue))
                {
                    //We found a start and an end, we need to add all of the values between those to the results
                    if (!Inclusive)
                    {
                        for (int j = startIndex + 1; j < i; j++)
                        {
                            results.Add(input[j]);
                        }
                    }
                    else
                    {
                        for (int j = startIndex; j <= i; j++)
                        {
                            results.Add(input[j]);
                        }
                    }

                    startIndex = -1;
                }
            }

            return results;
        }
    }
}
