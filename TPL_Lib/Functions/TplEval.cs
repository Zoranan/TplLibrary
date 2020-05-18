using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TplLib.Tpl_Parser;

namespace TplLib.Functions
{

    // PIPELINE SYNTAX
    //
    // eval fieldName = expressionOrValue
    // eval expressionOrValue as fieldName
    //

    /// <summary>
    /// Creates a new field, or updates an existing field using the expression specified
    /// </summary>
    public class TplEval : TplFunction
    {
        public string NewFieldName { get; internal set; }
        private ExpressionEV.Expression _evaluation;

        internal TplEval(string expression) { _evaluation = new ExpressionEV.Expression(expression); }

        //public TplEval(ParsableString query)
        //{
        //    query.GetNext(TokenType.VAR_NAME, false)
        //        .OnFailure(_ => throw new ArgumentException("You must assign the value of the Eval function to a field"))
        //        .OnSuccess(field => _newFieldName = field.Value())
                
        //        .GetNext("=")
                
        //        .OnFailure(_ => throw new ArgumentException($"Expected '=' after the field name '{_newFieldName}'"))
        //        .OnSuccess(eval =>
        //        {
        //            _evaluation = new ExpressionEV.Expression(eval.Source.GetRemainder().Value());
        //        });
        //}

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            foreach (var r in input)
            {
                foreach (var fn in _evaluation.VarNames)
                {
                    _evaluation.SetVar(fn, r.StringValueOf(fn));
                }

                r.AddOrUpdateField(NewFieldName, _evaluation.EvalAsString());
            }

            return input;
        }
    }
}
