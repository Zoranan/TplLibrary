using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Tpl_Parser.ExpressionTree.Operators.Binary
{
    #region Add Subtract
    internal class AdditionOperator : BinaryOperatorBase
    {
        internal AdditionOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();

            if (left is double lDbl && right is double rDbl) return lDbl + rDbl;
            if (left is bool lBool && right is bool rBool) return (lBool ? 1 : 0) + (rBool ? 1 : 0);
            if (left is bool lBool2 && right is double rDbl2) return (lBool2 ? 1 : 0) + rDbl2;
            if (left is double lDbl2 && right is bool rBool2) return lDbl2 + (rBool2 ? 1 : 0);
            return left as string ?? left.ToString() + right as string ?? right.ToString();
        }
    }

    internal class SubtractionOperator : BinaryOperatorBase
    {
        internal SubtractionOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();

            if (left is double lDbl && right is double rDbl) return lDbl - rDbl;
            if (left is bool lBool && right is bool rBool) return (lBool ? 1 : 0) - (rBool ? 1 : 0);    // Maybe throw an exception instead?
            if (left is bool lBool2 && right is double rDbl2) return (lBool2 ? 1 : 0) - rDbl2;
            if (left is double lDbl2 && right is bool rBool2) return lDbl2 - (rBool2 ? 1 : 0);
            return (left as string ?? left.ToString()).Replace(right as string ?? right.ToString(), "");   //Interesting concept... I wonder if other languages do this?
        }
    }
    #endregion

    #region Mutiply, Divide, Modulus

    internal class MultiplacationOperator : BinaryOperatorBase
    {
        internal MultiplacationOperator(ExpTreeNode parent) : base (parent)
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();

            if (left is double lDbl && right is double rDbl) return lDbl * rDbl;
            if (left is bool lBool && right is bool rBool) return (lBool ? 1 : 0) * (rBool ? 1 : 0);
            if (left is bool lBool2 && right is double rDbl2) return (lBool2 ? 1 : 0) * rDbl2;
            if (left is double lDbl2 && right is bool rBool2) return lDbl2 * (rBool2 ? 1 : 0);
            throw new InvalidOperationException($"Multiplication can not be performed on the operands [{left}] and [{right}]");
        }
    }

    internal class DivisionOperator : BinaryOperatorBase
    {
        internal DivisionOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();

            if (left is double lDbl && right is double rDbl) return lDbl / rDbl;
            if (left is bool lBool && right is bool rBool) return (lBool ? 1 : 0) / (rBool ? 1 : 0);
            if (left is bool lBool2 && right is double rDbl2) return (lBool2 ? 1 : 0) / rDbl2;
            if (left is double lDbl2 && right is bool rBool2) return lDbl2 / (rBool2 ? 1 : 0);
            throw new InvalidOperationException($"Division can not be performed on the operands [{left}] and [{right}]");
        }
    }

    internal class ModulusOperator : BinaryOperatorBase
    {
        internal ModulusOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();

            if (left is double lDbl && right is double rDbl) return lDbl % rDbl;
            if (left is bool lBool && right is bool rBool) return (lBool ? 1 : 0) % (rBool ? 1 : 0);
            if (left is bool lBool2 && right is double rDbl2) return (lBool2 ? 1 : 0) % rDbl2;
            if (left is double lDbl2 && right is bool rBool2) return lDbl2 % (rBool2 ? 1 : 0);
            throw new InvalidOperationException($"Modulus can not be performed on the operands [{left}] and [{right}]");
        }
    }

    #endregion

    #region Exponent

    internal class PowerOperator : BinaryOperatorBase
    {
        internal PowerOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();

            if (left is double lDbl && right is double rDbl) return Math.Pow(lDbl , rDbl);
            //if (left is bool lBool && right is bool rBool) return Math.Pow((lBool ? 1 : 0), (rBool ? 1 : 0));
            //if (left is bool lBool2 && right is double rDbl2) return Math.Pow((lBool2 ? 1 : 0), rDbl2);
            //if (left is double lDbl2 && right is bool rBool2) return Math.Pow(lDbl2, (rBool2 ? 1 : 0));
            throw new InvalidOperationException($"Power can not be performed on the operands [{left}] and [{right}]");
        }
    }
    #endregion

}
