using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TplLib.Tpl_Parser;

namespace TplLib.Functions.String_Functions
{
    public class TplSubstring : TplFunction
    {
        public int StartIndex { get; internal set; }
        public int MaxLength { get; internal set; } = -1;
        public string TargetField { get; internal set; } = TplResult.DEFAULT_FIELD;
        public string AsField { get; internal set; } = null;

        internal TplSubstring() { }

        public TplSubstring(int start, int length, string target, string asField)
        {
            StartIndex = start;
            MaxLength = length;
            TargetField = target;
            AsField = asField;
        }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            if (AsField == null)
                AsField = TargetField;

            var results = input.ToList();

            if (MaxLength == -1)
            {
                foreach (var i in results)
                {
                    try
                    {
                        var newVal = i.StringValueOf(TargetField).Substring(StartIndex);
                        i.AddOrUpdateField(AsField, newVal);
                    }
                    catch { i.AddOrUpdateField(AsField, ""); }
                }
            }
            else
            {
                foreach (var i in results)
                {
                    try
                    {
                        var newVal = i.StringValueOf(TargetField).Substring(StartIndex, MaxLength);
                        i.AddOrUpdateField(AsField, newVal);
                    }
                    catch { i.AddOrUpdateField(AsField, ""); }
                }
            }

            return results;
        }
    }
}
