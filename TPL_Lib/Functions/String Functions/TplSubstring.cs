using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPL_Lib.Tpl_Parser;

namespace TPL_Lib.Functions.String_Functions
{
    public class TplSubstring : TplFunction
    {
        private int StartIndex { get; set; }
        private int MaxLength { get; set; } = -1;
        private string TargetField { get; set; } = TplResult.DEFAULT_FIELD;
        private string AsField { get; set; } = null;


        public TplSubstring(int start, int length, string target, string asField)
        {
            StartIndex = start;
            MaxLength = length;
            TargetField = target;
            AsField = asField;
        }

        public TplSubstring()
        {

        }

        public TplSubstring(ParsableString query)
        {
            query.GetNext(TokenType.VAR_NAME)
                .OnSuccess(field => TargetField = field.Value())

                .GetNext(TokenType.INT)
                .OnSuccess(startIndex => StartIndex = startIndex.Value<int>())
                .OnFailure(_ => throw new ArgumentException($"Missing required integer argument for the start position in substring function"))

                .GetNext(TokenType.INT)
                .OnSuccess(length =>
                {
                    MaxLength = length.Value<int>();
                    if (MaxLength < 1)
                        throw new ArgumentException($"Invalid value for length argument in substring function. Value must be >= 1");
                })

                .GetNext("as")
                .OnSuccess(_as =>
                {
                    return _as.GetNext(TokenType.VAR_NAME)
                    .OnSuccess(asField => AsField = asField.Value())
                    .OnFailure(_ => throw new ArgumentException($"Expected field name after 'as' in substring function"));
                })
                .OnFailure(_ => AsField = TargetField)

                .Source.VerifyAtEnd();
        }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            var results = input.ToList();

            if (MaxLength == -1)
            {
                foreach (var i in results)
                {
                    try
                    {
                        var newVal = i.ValueOf(TargetField).Substring(StartIndex);
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
                        var newVal = i.ValueOf(TargetField).Substring(StartIndex, MaxLength);
                        i.AddOrUpdateField(AsField, newVal);
                    }
                    catch { i.AddOrUpdateField(AsField, ""); }
                }
            }

            return results;
        }
    }
}
