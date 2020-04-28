using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextFormatterLanguage;
using TPL_Lib.Tpl_Parser;

namespace TPL_Lib.Functions.String_Functions
{
    public class TplSplice : TplFunction
    {
        private Splicer _splicer;
        public string TargetField { get; private set; } = TplResult.DEFAULT_FIELD;
        public string AsField { get; private set; }

        public TplSplice(ParsableString query)
        {
            query.GetNext(TokenType.VAR_NAME)
                .OnSuccess(field => TargetField = field.Value())

                .GetNext(TokenType.QUOTE)
                .OnSuccess(splice => _splicer = new Splicer(splice.Value()))
                .OnFailure(_ => throw new ArgumentException($"Required argument for splice format is missing"))

                .GetNext("as")
                .OnSuccess(_as =>
                {
                    return _as.GetNext(TokenType.VAR_NAME)
                    .OnSuccess(asField => AsField = asField.Value());
                })
                .OnFailure(_ => AsField = TargetField)

                .Source.VerifyAtEnd();
        }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            var results = input.ToList();

            foreach(var r in results)
            {
                if (r.HasField(TargetField))
                {
                    r.AddOrUpdateField(AsField, _splicer.Format(r.ValueOf(TargetField)));
                }
            }

            return results;
        }
    }
}
