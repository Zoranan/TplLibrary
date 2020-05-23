using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Tpl_Parser.ExpressionTree
{
    internal class VariableValue : ValueNodeBase
    {
        internal readonly string Name;
        private object _value;

        internal VariableValue(string name, ExpTreeNode parent) : base (parent)
        {
            Name = name;
            RegisterVariable(name);
        }

        internal override void ClearAllVariables()
        {
            _value = null;
        }

        internal override void SetVariableValue(string varName, object value)
        {
            if (Name == varName)
                _value = value;
        }

        internal override object Eval()
        {
            if (_value == null)
                throw new ArgumentNullException($"Variable '{Name}' was in was not assigned a value");

            return _value;
        }
    }
}
