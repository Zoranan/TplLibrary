using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TplLib.Tpl_Parser;
using TplLib.Tpl_Parser.ExpressionTree;

namespace TplLib.Functions
{
    /// <summary>
    /// Creates a new field, or updates an existing field using the expression specified
    /// </summary>
    public class TplEval : TplFunction
    {
        public string NewFieldName { get; internal set; }
        private ExpTreeNode _evaluation;

        internal TplEval(ExpTreeNode expression) { _evaluation = expression; }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            Parallel.ForEach(input, result => 
            {
                _evaluation.ClearAllVariables();

                foreach (var varName in _evaluation.VarNames)
                {
                    var value = result.ValueOf(varName);

                    if (value == null)
                        return;

                    _evaluation.SetVariableValue(varName, value);
                }

                result.AddOrUpdateField(NewFieldName, _evaluation.Eval());
            });

            return input;
        }
    }
}
