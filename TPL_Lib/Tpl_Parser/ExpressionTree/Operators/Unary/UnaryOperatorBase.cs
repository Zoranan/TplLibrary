using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Tpl_Parser.ExpressionTree.Operators.Unary
{
    internal abstract class UnaryOperatorBase : OperatorBase
    {
        internal ExpTreeNode Operand
        {
            get => _children[0];
            set { _children[0] = value; }
        }
        internal UnaryOperatorBase(ExpTreeNode parent) : base(parent) 
        {
            _children.Add(null);
        }
    }
}
