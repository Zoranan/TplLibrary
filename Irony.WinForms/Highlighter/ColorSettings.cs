using FastColoredTextBoxNS;
using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irony.WinForms.Highlighter
{
    public class ColorSettings
    {
        public TextStyle Default = new TextStyle(Brushes.White, null, FontStyle.Regular);
        public TextStyle Comment;
        public TextStyle Identifier;
        public TextStyle Keyword;
        public TextStyle Number;
        public TextStyle String;
        public TextStyle Text;
    }
}
