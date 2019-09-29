using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPL_Lib.Extensions;
using TPL_Lib.Tpl_Parser;

namespace TPL_Lib.Functions
{
    // PIPELINE SYNTAX
    //
    // where fieldName == "Value"
    // where CONDITION == TRUE
    //

    /// <summary>
    /// Removes all fields from a list of TplResults EXCEPT the ones that meet the specified condition
    /// </summary>
    public class TplWhere : TplFunction
    {
        private ExpressionEV.Expression _condition;

        public TplWhere (ParsableString query)
        {
            _condition = new ExpressionEV.Expression(query.GetRemainder().Value());
        }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            for (int i=0; i<input.Count; i++)
            {
                bool eval = true;

                //Set the variables in our condition to what we have in this TplResult
                foreach (var varName in _condition.VarNames)
                {
                    var value = input[i].ValueOf(varName);
                    _condition.SetVar(varName, value);
                }

                //Evaluate the condition. If it is false, remove this TplResult
                if (eval && !_condition.EvalAsBool())
                {
                    input.RemoveAt(i);
                    i--;
                }
            }

            return input;
        }
    }
}
