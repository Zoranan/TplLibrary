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
    // PIPELINE SYNTAX
    //
    // replace "this" "that"
    // replace "tHiS" "that" matchcase=false
    // replace "^th.." "that" mode=rex
    // replace fieldName "^th.." "that"
    // etc...
    //

    /// <summary>
    /// Finds and replaces the first quoted phrase, with another
    /// In search mode, accepts wild card *
    /// </summary>
    public class TplReplace : TplFunction
    {
        private bool _caseSensitive = false;
        private bool _regexMode = false;

        private Regex _find;
        private string _replace;
        private string _targetGroup;

        private List<string> _targetFields = new List<string>() { TplResult.DEFAULT_FIELD };
        private string _asField;

        #region Constructors
        public TplReplace (string find, string replace, string asField, bool caseSensitive, bool regexMode)
        {
            //Process find
            if (!regexMode)
            {
                find = TplSearch._escapedWildCardRegex.Replace(Regex.Escape(find), ".*?");
            }

            RegexOptions options = RegexOptions.Compiled;
            if (caseSensitive)
                options = options | RegexOptions.IgnoreCase;

            _find = new Regex(find, options);

            //Set other vars
            _replace = replace;
            _asField = asField;
            _caseSensitive = caseSensitive;
            _regexMode = regexMode;
        }

        public TplReplace(ParsableString query)
        {
            string findTerm = null;
            bool caseWasSet = false;
            bool modeWasSet = false;

            query.GetNextList(TokenType.VAR_NAME)
                .OnSuccess(fields => _targetFields = fields.ResultsList.Select(f => f.Value()).ToList())

                .GetNext(TokenType.QUOTE)
                .OnFailure(_ => throw new ArgumentException($"Missing search term / regex to replace in replace function"))
                .OnSuccess(find => findTerm = find.Value())

                .GetNext(TokenType.QUOTE)
                .OnFailure(_ => throw new ArgumentException($"Missing replacement term in replace function"))
                .OnSuccess(replace => _replace = replace.Value())

                .WhileGetNext(TokenType.PARAMETER, param =>
                {
                    switch (param.ParamName.Value().ToLower())
                    {
                        case "group":
                            _targetGroup = param.ParamValue.Value();
                            _regexMode = true;
                            break;

                        case "casesensitive":
                            _caseSensitive = param.ParamValue.Value<bool>();
                            caseWasSet = true;
                            break;

                        case "mode":
                            modeWasSet = true;
                            switch (param.ParamValue.Value().ToLower())
                            {
                                case "rex":
                                    _regexMode = true;

                                    if (!caseWasSet)
                                        _caseSensitive = true;

                                    break;
                                case "normal":
                                    _regexMode = false;
                                    break;
                                default:
                                    throw new ArgumentException($"\"Replace\" function \"mode\" parameter does not take a value of {param.ParamValue}");
                            }
                            break;
                    }
                })

                .GetNext("as")
                .OnSuccess(_as =>
                {
                    return _as.GetNext(TokenType.VAR_NAME)
                    .OnSuccess(asField => _asField = asField.Value())
                    .OnFailure(_ => throw new ArgumentException($"Expected a field name after 'as' in replace function"));
                })

                .Source.VerifyAtEnd();

            //Final checking
            if (_targetGroup != null && !_regexMode)
            {
                if (modeWasSet)
                    throw new ArgumentException($"Replace function must be in Regex mode to use the group parameter");

                else
                    _regexMode = true;
            }

            if (!_regexMode)
            {
                findTerm = Regex.Escape(findTerm);
                
                if (!caseWasSet)
                    _caseSensitive = false;
            }

            var options = RegexOptions.Compiled;
            if (!_caseSensitive)
                options = RegexOptions.Compiled | RegexOptions.IgnoreCase;

            _find = new Regex(findTerm, options);
            if (_targetGroup != null && !_find.GetNamedCaptureGroupNames().Contains(_targetGroup))
                throw new ArgumentException($"The target group '{_targetGroup}' does not exist in the find regex, in the replace function");
        }

        #endregion

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            if (_asField == null)
            {

                foreach (var i in input)
                {
                    foreach (var f in _targetFields)
                    {
                        if (i.HasField(f))
                        {
                            if (_targetGroup == null)
                            {
                                var newValue = _find.Replace(i.ValueOf(f), _replace);
                                i.AddOrUpdateField(f, newValue);
                            }
                            else
                            {
                                var newValue = _find.ReplaceGroup(i.ValueOf(f), _replace, _targetGroup);
                                i.AddOrUpdateField(f, newValue);
                            }
                        }
                    }
                }
            }

            else
            {

                foreach (var i in input)
                {
                    if (i.HasField(_targetFields[0]))
                    {
                        if (_targetGroup == null)
                        {
                            var newValue = _find.Replace(i.ValueOf(_targetFields[0]), _replace);
                            i.AddOrUpdateField(_asField, newValue);
                        }
                        else
                        {
                            var newValue = _find.ReplaceGroup(i.ValueOf(_targetFields[0]), _replace, _targetGroup);
                            i.AddOrUpdateField(_asField, newValue);
                        }
                    }
                }
            }

            return input;
        }
    }
}
