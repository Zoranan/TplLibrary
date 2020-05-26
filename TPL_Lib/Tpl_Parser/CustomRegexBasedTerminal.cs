#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Irony;
using Irony.Parsing;

namespace TplParser
{

    //Note: this class was not tested at all
    // Based on contributions by CodePlex user sakana280
    // 12.09.2008 - breaking change! added "name" parameter to the constructor
    public class CustomRegexBasedTerminal : Terminal
    {
        #region Public Fields
        public readonly string Pattern;
        public readonly StringList Prefixes = new StringList();
        #endregion

        public Regex Expression { get; private set; }
        public Func<Match, object> ValueSelector { get; set; }

        #region Constructors
        public CustomRegexBasedTerminal(string pattern, params string[] prefixes) : base("name")
        {
            Pattern = pattern;
            if (prefixes != null)
                Prefixes.AddRange(prefixes);
        }
        public CustomRegexBasedTerminal(string name, string pattern, params string[] prefixes) : base(name)
        {
            Pattern = pattern;
            if (prefixes != null)
                Prefixes.AddRange(prefixes);
        }
        #endregion

        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
            string workPattern = @"\G(" + Pattern + ")";
            RegexOptions options = (Grammar.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
            Expression = new Regex(workPattern, options); // TODO: Add compiled?
            if (EditorInfo == null)
                EditorInfo = new TokenEditorInfo(TokenType.Unknown, TokenColor.Text, TokenTriggers.None);
        }

        public override IList<string> GetFirsts()
        {
            return Prefixes;
        }

        public override Token TryMatch(ParsingContext context, ISourceStream source)
        {
            Match m = Expression.Match(source.Text, source.PreviewPosition);
            if (!m.Success || m.Index != source.PreviewPosition)
                return null;

            source.PreviewPosition += m.Length;

            if (ValueSelector != null)
                return source.CreateToken(this, ValueSelector.Invoke(m));

            return source.CreateToken(OutputTerminal);
        }

    }//class

}//namespace