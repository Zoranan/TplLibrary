using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Tpl_Parser.ExpressionTree
{
    internal abstract class ValueNodeBase : ExpTreeNode
    {
        internal ValueNodeBase(ExpTreeNode parent) : base(parent) { }
    }
}
