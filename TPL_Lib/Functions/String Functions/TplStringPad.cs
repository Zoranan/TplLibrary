using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPL_Lib.Tpl_Parser;

namespace TPL_Lib.Functions.String_Functions
{
    // PIPELINE SYNTAX
    // Pad 10                                         |   Pads _raw on left to length 10, filling with spaces
    // Pad length=10,                                 |   Pads _raw on left to length 10, filling with spaces
    // Pad field1, field2 mode=left length=10,        |   Pads field1 and field2 on left to length 10, filling with spaces
    // Pad field1, field2 mode=left length=10, "0"    |   Pads field1 and field2 on left to length 10, filling with 0's
    //

    /// <summary>
    /// Sorts a list of TplResults by one or more of their fields
    /// </summary>
    public class TplStringPad : TplFunction
    {
        private int _totalLength;
        private char _paddingChar;
        private bool _padRight;
        private List<string> _targetFields;

        private TplStringPad (int length, char pad, List<string> fields = null, bool padRight = false)
        {
            _totalLength = length;
            _paddingChar = pad;
            _targetFields = fields;
            _padRight = padRight;
        }

        public TplStringPad(ParsableString query)
        {
            query.GetNext(TokenType.WORD)
                .OnSuccess(leftRight =>
                {
                    switch (leftRight.Value().ToLower())
                    {
                        case "left":
                            _padRight = false;
                            return leftRight;

                        case "right":
                            _padRight = true;
                            return leftRight;

                        default:
                            return TokenResult.Fail(leftRight.Source);
                    }
                })
                .OnFailure(_ => throw new ArgumentException($"Pad function requires a direction to be specified as the first argument. Acceptable values are 'left' or 'right'"))

                .GetNextList(TokenType.VAR_NAME)
                .OnSuccess(fields => _targetFields = fields.ResultsList.Select(f => f.Value()).ToList())
                .OnFailure(_ => _targetFields = new List<string>() { TplResult.DEFAULT_FIELD })

                .GetNext(TokenType.QUOTE)
                .OnSuccess(charQuote =>
                {
                    if (charQuote.Value().Length != 1)
                        throw new ArgumentException($"The padding character argument can only be a length of 1");

                    _paddingChar = charQuote.Value()[0];
                })
                .OnFailure(_ => _paddingChar = ' ')

                .GetNext(TokenType.INT)
                .OnFailure(_ => throw new ArgumentException($"An integer value must be specified for the padding length in the pad function"))
                .OnSuccess(length => _totalLength = length.Value<int>())

                .Source.VerifyAtEnd();
        }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            //Pad right
            if (_padRight)
            {
                foreach (var i in input)
                {
                    foreach (var f in _targetFields)
                    {
                        if (i.HasField(f))
                        {
                            var padded = i.ValueOf(f).PadRight(_totalLength, _paddingChar);
                            i.AddOrUpdateField(f, padded);
                        }
                    }
                }
            }

            //Pad left
            else
            {
                foreach (var i in input)
                {
                    foreach (var f in _targetFields)
                    {
                        if (i.HasField(f))
                        {
                            var padded = i.ValueOf(f).PadLeft(_totalLength, _paddingChar);
                            i.AddOrUpdateField(f, padded);
                        }
                    }
                }
            }

            return input;
        }
    }
}
