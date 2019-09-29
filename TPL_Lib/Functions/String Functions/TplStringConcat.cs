using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TPL_Lib.Extensions;
using TPL_Lib.Tpl_Parser;

namespace TPL_Lib.Functions.String_Functions
{
    public class TplStringConcat : TplFunction
    {
        private List<string> _strings = new List<string>();
        private List<string> _fields = new List<string>();

        private string _asField;

        public TplStringConcat(List<string> strings, List<string> fields, string asField)
        {
            _strings = strings;
            _fields = fields;
            _asField = asField;
        }

        public TplStringConcat(ParsableString query)
        {
            var currentToken = query.GetNext();

            while (currentToken.IsSuccess && (currentToken.IsType(TokenType.QUOTE) || currentToken.Value() != "as"))
            {
                if (currentToken.IsType(TokenType.QUOTE) || currentToken.IsType(TokenType.DECIMAL))
                {
                    _strings.Add(currentToken.Value());
                    _fields.Add(null);
                }
                else if (currentToken.IsType(TokenType.VAR_NAME))
                {
                    _strings.Add(null);
                    _fields.Add(currentToken.Value());
                }
                else
                {
                    throw new ArgumentException($"Invalid token '{currentToken.Value()}' in concat function");
                }

                currentToken = currentToken.GetNext();
            }

            currentToken
                .OnFailure(_ => throw new ArgumentException($"Assignment field is missing from concat function"))
                
                .GetNext(TokenType.VAR_NAME)
                .OnSuccess(asField => _asField = asField.Value())
                .OnFailure(_ => throw new ArgumentException($"Expected a field name after 'as' in concat function"));
        }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            foreach (var i in input)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < _strings.Count; j++)
                {
                    if (_strings[j] != null && _fields[j] == null)
                    {
                        sb.Append(_strings[j]);
                    }

                    else if (_strings[j] == null && _fields[j] != null)
                    {
                        sb.Append(i.ValueOf(_fields[j]));
                    }

                    else
                    {
                        throw new Exception("Concat encountered a runtime error when keeping the order of arguments to be concatenated!");
                    }
                }

                i.AddOrUpdateField(_asField, sb.ToString());
            }

            return input;
        }
    }
}
