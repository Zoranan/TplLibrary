using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Tpl_Parser.ExpressionTree
{
    /// <summary>
    /// Specifies what object type the node produces when 'Eval' is called
    /// </summary>
    internal enum EvalType
    {
        /// <summary>
        /// This node produces a numeric (double) value
        /// </summary>
        Number,
        /// <summary>
        /// This node produces a boolean value
        /// </summary>
        Boolean,
        /// <summary>
        /// This node is not an operator (literal or variable)
        /// </summary>
        Value,
    }
    abstract class ExpTreeNode
    {
        internal readonly ExpTreeNode Parent;

        private readonly List<string> _varNames = new List<string>();
        internal IReadOnlyList<string> VarNames { get => _varNames; }
        
        protected List<ExpTreeNode> _children = new List<ExpTreeNode>();
        internal IReadOnlyList<ExpTreeNode> Children { get => _children; }
        
        internal ExpTreeNode Root { get { return Parent?.Root ?? this; } }

        internal ExpTreeNode(ExpTreeNode parent)
        {
            Parent = parent;
        }

        internal abstract object Eval();

        internal bool EvalAsBool()
        {
            var eval = Eval();

            if (eval is bool b) return b;
            if (eval is double d) return d != 0;
            else return !string.IsNullOrEmpty(eval.ToString());
        }

        internal virtual void ClearAllVariables()
        {
            foreach (var child in Children)
                child.ClearAllVariables();
        }

        internal virtual void SetVariableValue(string varName, object value)
        {
            foreach (var child in Children)
                child.SetVariableValue(varName, value);
        }

        protected void RegisterVariable(string varName)
        {
            Root._varNames.Add(varName);
        }
    }
}
