using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TplLib.Tpl_Parser;

namespace TplLib.Functions.String_Functions
{
    /// <summary>
    /// Sorts a list of TplResults by one or more of their fields
    /// </summary>
    public class TplStringPad : TplFunction
    {
        public int TotalLength { get; internal set; }
        public char PaddingChar { get; internal set; }
        public bool PadRight { get; internal set; }
        public List<string> TargetFields { get; internal set; }

        internal TplStringPad() { }

        private TplStringPad (int length, char pad, List<string> fields = null, bool padRight = false)
        {
            TotalLength = length;
            PaddingChar = pad;
            TargetFields = fields;
            PadRight = padRight;
        }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            Parallel.ForEach(input, result =>
            {
                foreach (var field in TargetFields)
                {
                    if (result.HasField(field))
                    {
                        var val = result.StringValueOf(field);
                        result.AddOrUpdateField(field, PadRight ? val.PadRight(TotalLength, PaddingChar) : val.PadLeft(TotalLength, PaddingChar));
                    }
                }
            });
            return input;
        }
    }
}
