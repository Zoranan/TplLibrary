using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TplLib.Extensions;
using TplLib.Tpl_Parser;
using TplLib.Tpl_Parser.ExpressionTree;

namespace TplLib.Functions
{
    /// <summary>
    /// Removes all fields from a list of TplResults EXCEPT the ones that meet the specified condition
    /// </summary>
    public class TplWhere : TplFunction
    {
        private ExpTreeNode _condition;

        internal TplWhere(ExpTreeNode condition) { _condition = condition; }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            for (int i=0; i<input.Count; i++)
            {
                //Set the variables in our condition to what we have in this TplResult
                foreach (var varName in _condition.VarNames)
                    _condition.SetVariableValue(varName, input[i].ValueOf(varName));

                //Evaluate the condition. If it is false, remove this TplResult
                if (!_condition.EvalAsBool())
                {
                    input.RemoveAt(i);
                    i--;
                }
            }

            return input;
        }
    }
}
