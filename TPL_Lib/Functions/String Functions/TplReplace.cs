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
                options |= RegexOptions.IgnoreCase;

            _find = new Regex(regex, options);
        }

        #endregion

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            Parallel.ForEach(input, result =>
            {
                foreach (var field in TargetFields)
                {
                    if (result.HasField(field))
                    {
                        if (TargetGroup == null)
                        {
                            var newValue = _find.Replace(result.StringValueOf(field), Replace);
                            result.AddOrUpdateField(AsField ?? field, newValue);
                        }
                        else
                        {
                            var newValue = _find.ReplaceGroup(result.StringValueOf(field), Replace, TargetGroup);
                            result.AddOrUpdateField(AsField ?? field, newValue);
                        }
                    }
                }
            });

            return input;
        }
    }
}
