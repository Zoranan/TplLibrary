using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Tpl_Parser.ExpressionTree.Operators.Unary
{
    #region Prefix Bool
    internal class NotOperator : UnaryOperatorBase
    {
        internal NotOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            var operand = Operand.Eval();

            if (operand is double dbl) return dbl == 0;
            if (operand is bool b) return !b;
            return string.IsNullOrEmpty(operand as string ?? operand.ToString());
        }
    }

    #endregion

    //internal class SqaureRootOperator : UnaryOperatorBase
    //{
    //    internal SqaureRootOperator(ExpTreeNode parent) : base(parent)
    //    {
    //        //
    //    }

    //    internal override object Eval()
    //    {
    //        var operand = Operand.Eval();

    //        if (operand is double dbl) return Math.Sqrt(dbl);
    //        if (operand is bool b) return Math.Sqrt(b? 1 : 0);
    //        throw new InvalidOperationException($"Square root can not be performed on the operand [{operand}]");
    //    }
    //}

    internal class GenericUnaryMathFunctionOperator : UnaryOperatorBase
    {
        private readonly string FunctionName;
        private readonly Func<double, double> MathFunc;
        internal GenericUnaryMathFunctionOperator(string functionName, ExpTreeNode parent) : base(parent)
        {
            functionName = $"{functionName.Substring(0, 1).ToUpper()}{functionName.Substring(1).ToLower()}";
            FunctionName = functionName;
            var method = typeof(Math).GetMethod(functionName, BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(double) }, null);

            if (method == null)
                throw new InvalidOperationException($"Invalid function name '{functionName}'");

            MathFunc = (Func<double, double>)method.CreateDelegate(typeof(Func<double, double>));
        }

        internal override object Eval()
        {
            var operand = Operand.Eval();

            try
            {
                if (operand is double dbl)
                    return MathFunc.Invoke(dbl);

                if (operand is bool b)
                    return MathFunc.Invoke(b ? 1 : 0);

                throw new InvalidOperationException($"{FunctionName} can not be performed on the operand [{operand}]");
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }
    }
}
