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
        private readonly ExpTreeNode _condition;

        internal TplWhere(ExpTreeNode condition) { _condition = condition; }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            for (int i=0; i<input.Count; i++)
            {
                var allVarsSet = true;

                //Set the variables in our condition to what we have in this TplResult
                foreach (var varName in _condition.VarNames)
                {
                    var value = input[i].ValueOf(varName);

                    if (value == null)
                        allVarsSet = false;

                    else
                        _condition.SetVariableValue(varName, value);
                }

                //Evaluate the condition. If it is false or contains null variables, remove this TplResult
                if (!allVarsSet || !_condition.EvalAsBool())
                {
                    input.RemoveAt(i);
                    i--;
                }

                _condition.ClearAllVariables();
            }

            return input;
        }
    }
}
