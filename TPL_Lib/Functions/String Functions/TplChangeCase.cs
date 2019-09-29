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
    public class TplChangeCase : TplFunction
    {
        private List<string> _targetFields;
        private string _targetGroup = null;
        private string _asField;
        private Regex _matchingRegex = null;

        public bool ToUpper { get; set; } = false;

        public TplChangeCase(ParsableString query)
        {
            query.GetNextList(TokenType.VAR_NAME)
                .OnSuccess(fields => _targetFields = fields.ResultsList.Select(f => f.Value()).ToList())
                .OnFailure(_ => _targetFields = new List<string>() { TplResult.DEFAULT_FIELD })

                .GetNext(TokenType.QUOTE)
                .OnSuccess(rex => _matchingRegex = new Regex(rex.Value(), RegexOptions.Compiled))

                .GetNext(TokenType.PARAMETER)
                .OnSuccess(param =>
                {
                    if (param.ParamName.Value().ToLower() == "group")
                        _targetGroup = param.ParamValue.Value();

                    else throw new ArgumentException($"Invalid parameter {param.ParamName} in change case function");
                })

                .GetNext("as")
                .OnSuccess(_as =>
                {
                    return _as.GetNext(TokenType.VAR_NAME)
                    .OnSuccess(asField => _asField = asField.Value())
                    .OnFailure(_ => throw new ArgumentException($"Expected a field name after 'as' in change case function"));
                })

                .Source.VerifyAtEnd();

            //Final checks
            if (_matchingRegex == null && _targetGroup != null)
                throw new ArgumentException($"Target group cannot be set in change case function unless a regex is specified");

            if (_matchingRegex != null && _targetGroup != null && !_matchingRegex.GetNamedCaptureGroupNames().Contains(_targetGroup))
                throw new ArgumentException($"The matching regex in change case function does not contain the named capture group '{_targetGroup}'");
        }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            //No matching regex was specified, so match the whole thing
            if (_matchingRegex == null && _asField == null)
            {
                foreach (var i in input)
                {
                    if (ToUpper)
                    {
                        foreach (var f in _targetFields)
                        {
                            i.AddOrUpdateField(f, i.ValueOf(f).ToUpper());
                        }
                    }
                    else
                    {
                        foreach (var f in _targetFields)
                        {
                            i.AddOrUpdateField(f, i.ValueOf(f).ToLower());
                        }
                    }
                }
            }

            else if (_matchingRegex == null)
            {
                foreach (var i in input)
                {
                    if (ToUpper)
                    {
                        i.AddOrUpdateField(_asField, i.ValueOf(_targetFields[0]).ToUpper());
                    }
                    else
                    {
                        i.AddOrUpdateField(_asField, i.ValueOf(_targetFields[0]).ToLower());
                    }
                }
            }

            //We definitely have a matching regex
            //We may have an as field

            else
            {
                foreach (var i in input)
                {
                    foreach (var f in _targetFields)
                    {
                        var fValue = i.ValueOf(f);
                        var matches = _matchingRegex.Matches(fValue);
                        StringBuilder sb = new StringBuilder();

                        //Add the initial bit of our string if needed
                        if (matches.Count > 0 && matches[0].Index > 0)
                        {
                            if (_targetGroup == null)
                                sb.Append(fValue.Substring(0, matches[0].Index));

                            else
                                sb.Append(fValue.Substring(0, matches[0].Groups[_targetGroup].Index));
                        }

                        foreach (Match m in matches)
                        {
                            string val = "";
                            //bool skip = false;

                            if (_targetGroup == null)
                                val = m.Value;

                            else if (_targetGroup != null && m.Groups[_targetGroup].Success)
                                val = m.Groups[_targetGroup].Value;

                            else
                                continue;

                            if (_targetGroup == null)
                                sb.Append(fValue.Substring(sb.Length, m.Index - sb.Length));

                            else
                                sb.Append(fValue.Substring(sb.Length, m.Groups[_targetGroup].Index - sb.Length));

                            if (ToUpper)
                            {
                                sb.Append(val.ToUpper());
                            }
                            else
                            {
                                sb.Append(val.ToLower());
                            }

                        }

                        //Add the remaining bit of our string if needed
                        if (sb.Length < fValue.Length)
                            sb.Append(fValue.Substring(sb.Length));

                        //Store it
                        if (_asField == null)
                            i.AddOrUpdateField(f, sb.ToString());

                        else
                            i.AddOrUpdateField(_asField, sb.ToString());
                    }
                }
            }

            return input;
        }
    }
}
