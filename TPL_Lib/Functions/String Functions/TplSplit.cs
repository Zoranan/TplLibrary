using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Functions.String_Functions
{
    public class TplSplit : TplFunction
    {
        public string TargetField { get; internal set; }
        public string SplitOn { get; internal set; }

        internal TplSplit() { }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            Parallel.ForEach(input, result =>
            {
                var splits = result.StringValueOf(TargetField).Split(new string[] { SplitOn }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < splits.Length; i++)
                    result.AddField((i + 1).ToString(), splits[i]);
            });

            return input;
        }
    }
}
