using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Tpl_Parser.ExpressionTree.Operators
{
    internal abstract class OperatorBase : ExpTreeNode
    {
        internal OperatorBase(ExpTreeNode parent) : base(parent) { }
    }
}
