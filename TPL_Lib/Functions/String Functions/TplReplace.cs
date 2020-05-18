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
    /// <summary>
    /// Finds and replaces the first quoted phrase, with another
    /// In search mode, accepts wild card *
    /// </summary>
    public class TplReplace : TplFunction
    {
        private Regex _find;

        public enum ReplaceMode { REX, NORMAL }
        public bool CaseSensitive { get; internal set; } = false;
        public bool RegexMode { get; internal set; } = false;
        public string Find 
        {
            get => _find.ToString();
            internal set 
            {
                InitRegex(value);
            }
        }
        public string Replace { get; internal set; }
        public string TargetGroup { get; internal set; }

        public List<string> TargetFields { get; internal set; } = new List<string>() { TplResult.DEFAULT_FIELD };
        public string AsField { get; internal set; }

        #region Constructors
        internal TplReplace() { }

        public TplReplace (string find, string replace, string asField, bool caseSensitive, bool regexMode)
        {
            InitRegex(find);

            //Set other vars
            Replace = replace;
            AsField = asField;
            CaseSensitive = caseSensitive;
            RegexMode = regexMode;
        }

        internal void InitRegex(string regex)
        {
            if (!RegexMode)
                regex = Regex.Escape(regex).Replace(@"\\*", ".*?");

            RegexOptions options = RegexOptions.Compiled;
            if (CaseSensitive)
                options = options | RegexOptions.IgnoreCase;

            _find = new Regex(regex, options);
        }

        //public TplReplace(ParsableString query)
        //{
        //    string findTerm = null;
        //    bool caseWasSet = false;
        //    bool modeWasSet = false;

        //    query.GetNextList(TokenType.VAR_NAME)
        //        .OnSuccess(fields => _targetFields = fields.ResultsList.Select(f => f.Value()).ToList())

        //        .GetNext(TokenType.QUOTE)
        //        .OnFailure(_ => throw new ArgumentException($"Missing search term / regex to replace in replace function"))
        //        .OnSuccess(find => findTerm = find.Value())

        //        .GetNext(TokenType.QUOTE)
        //        .OnFailure(_ => throw new ArgumentException($"Missing replacement term in replace function"))
        //        .OnSuccess(replace => _replace = replace.Value())

        //        .WhileGetNext(TokenType.PARAMETER, param =>
        //        {
        //            switch (param.ParamName.Value().ToLower())
        //            {
        //                case "group":
        //                    _targetGroup = param.ParamValue.Value();
        //                    _regexMode = true;
        //                    break;

        //                case "casesensitive":
        //                    _caseSensitive = param.ParamValue.Value<bool>();
        //                    caseWasSet = true;
        //                    break;

        //                case "mode":
        //                    modeWasSet = true;
        //                    switch (param.ParamValue.Value().ToLower())
        //                    {
        //                        case "rex":
        //                            _regexMode = true;

        //                            if (!caseWasSet)
        //                                _caseSensitive = true;

        //                            break;
        //                        case "normal":
        //                            _regexMode = false;
        //                            break;
        //                        default:
        //                            throw new ArgumentException($"\"Replace\" function \"mode\" parameter does not take a value of {param.ParamValue}");
        //                    }
        //                    break;
        //            }
        //        })

        //        .GetNext("as")
        //        .OnSuccess(_as =>
        //        {
        //            return _as.GetNext(TokenType.VAR_NAME)
        //            .OnSuccess(asField => _asField = asField.Value())
        //            .OnFailure(_ => throw new ArgumentException($"Expected a field name after 'as' in replace function"));
        //        })

        //        .Source.VerifyAtEnd();

        //    //Final checking
        //    if (_targetGroup != null && !_regexMode)
        //    {
        //        if (modeWasSet)
        //            throw new ArgumentException($"Replace function must be in Regex mode to use the group parameter");

        //        else
        //            _regexMode = true;
        //    }

        //    if (!_regexMode)
        //    {
        //        findTerm = Regex.Escape(findTerm);
                
        //        if (!caseWasSet)
        //            _caseSensitive = false;
        //    }

        //    var options = RegexOptions.Compiled;
        //    if (!_caseSensitive)
        //        options = RegexOptions.Compiled | RegexOptions.IgnoreCase;

        //    _find = new Regex(findTerm, options);
        //    if (_targetGroup != null && !_find.GetNamedCaptureGroupNames().Contains(_targetGroup))
        //        throw new ArgumentException($"The target group '{_targetGroup}' does not exist in the find regex, in the replace function");
        //}

        #endregion

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            if (AsField == null)
            {

                foreach (var i in input)
                {
                    foreach (var f in TargetFields)
                    {
                        if (i.HasField(f))
                        {
                            if (TargetGroup == null)
                            {
                                var newValue = _find.Replace(i.StringValueOf(f), Replace);
                                i.AddOrUpdateField(f, newValue);
                            }
                            else
                            {
                                var newValue = _find.ReplaceGroup(i.StringValueOf(f), Replace, TargetGroup);
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
                    if (i.HasField(TargetFields[0]))
                    {
                        if (TargetGroup == null)
                        {
                            var newValue = _find.Replace(i.StringValueOf(TargetFields[0]), Replace);
                            i.AddOrUpdateField(AsField, newValue);
                        }
                        else
                        {
                            var newValue = _find.ReplaceGroup(i.StringValueOf(TargetFields[0]), Replace, TargetGroup);
                            i.AddOrUpdateField(AsField, newValue);
                        }
                    }
                }
            }

            return input;
        }
    }
}
