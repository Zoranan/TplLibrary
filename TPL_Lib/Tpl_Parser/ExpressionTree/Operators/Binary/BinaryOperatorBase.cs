using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Tpl_Parser.ExpressionTree.Operators.Binary
{
    internal abstract class BinaryOperatorBase : OperatorBase
    {
        internal ExpTreeNode LeftOperand 
        { 
            get => _children[0]; 
            set { _children[0] = value; } 
        }
        internal ExpTreeNode RightOperand 
        { 
            get => _children[1]; 
            set { _children[1] = value; } 
        }

        internal BinaryOperatorBase(ExpTreeNode parent) : base(parent) 
        {
            _children.Add(null);
            _children.Add(null);
        }
    }
}
