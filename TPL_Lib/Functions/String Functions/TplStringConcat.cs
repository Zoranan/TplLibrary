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
    public class TplStringConcat : TplFunction
    {
        public class ConcatValue
        {
            public readonly string Value;
            public readonly bool IsVar;

            public ConcatValue(string val, bool isVar)
            {
                Value = val;
                IsVar = isVar;
            }
        }
        public IReadOnlyList<ConcatValue> ConcatValues { get; internal set; } = new List<ConcatValue>();
        public string AsField { get; internal set; }

        internal TplStringConcat() { }

        public TplStringConcat(List<ConcatValue> values, string asField)
        {
            ConcatValues = values;
            AsField = asField;
        }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            Parallel.ForEach(input, result =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (var value in ConcatValues)
                    sb.Append(value.IsVar ? result.StringValueOf(value.Value) : value.Value);

                result.AddOrUpdateField(AsField, sb.ToString());
            });

            return input;
        }
    }
}
