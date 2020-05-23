using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Tpl_Parser.ExpressionTree
{
    internal class LiteralValue : ValueNodeBase
    {
        internal readonly object Value;



        internal override void ClearAllVariables()
        {
            // No variable here, do nothing
        }

        internal override void SetVariableValue(string varName, object value)
        {
            // No variable here, do nothing
        }

        internal LiteralValue (object value, ExpTreeNode parent) : base(parent)
        {
            Value = value;
        }

        internal override object Eval()
        {
            return Value;
        }
    }
}
