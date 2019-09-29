using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPL_Lib.Tpl_Parser;

namespace TPL_Lib.Functions
{
    public class TplBetween : TplFunction
    {
        private string _targetField;
        private string _startValue;
        private string _endValue;
        private bool _inclusive = false;

        public TplBetween(string targetField, string startVal, string endVal, bool inclusive = false)
        {
            _targetField = targetField;
            _startValue = startVal;
            _endValue = endVal;
            _inclusive = inclusive;
        }

        public TplBetween (ParsableString query)
        {
            query.GetNext(TokenType.VAR_NAME)
                .OnSuccess(field => _targetField = field.Value())
                .OnFailure(_ => _targetField = TplResult.DEFAULT_FIELD)

                .GetNext(TokenType.QUOTE)
                .OnSuccess(start => _startValue = start.Value())
                .OnFailure(_ => throw new ArgumentException($"Required argument for start value was missing from between function"))

                .GetNext(TokenType.QUOTE)
                .OnSuccess(end => _endValue = end.Value())
                .OnFailure(_ => throw new ArgumentException($"Required argument for end value was missing from between function"))

                .GetNext(TokenType.PARAMETER)
                .OnSuccess(param =>
                {
                    if (param.ParamName.Value() != "inclusive")
                        throw new ArgumentException($"Between function does not take parameter '{param.ParamName}'");

                    _inclusive = param.ParamValue.Value<bool>();
                })

                .Source.VerifyAtEnd();
        }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            var results = new List<TplResult>();
            int startIndex = -1;

            for (int i = 0; i < input.Count; i++)
            {
                var val = input[i].ValueOf(_targetField);

                if (val.Contains(_startValue))
                    startIndex = i;

                if (startIndex != -1 && val.Contains(_endValue))
                {
                    //We found a start and an end, we need to add all of the values between those to the results
                    if (!_inclusive)
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
