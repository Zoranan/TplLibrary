using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TplLib.Tpl_Parser.ExpressionTree.Operators.Binary
{
    internal class EqualsOperator : BinaryOperatorBase
    {
        internal EqualsOperator(ExpTreeNode parent) : base(parent) // ==
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();

            return left.Equals(right);
        }
    }

    internal class NotEqualsOperator : BinaryOperatorBase
    {
        internal NotEqualsOperator(ExpTreeNode parent) : base(parent) // !=
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();

            return !left.Equals(right);
        }
    }

    internal class LooseEqualsOperator : BinaryOperatorBase // ~=
    {
        internal LooseEqualsOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();

            if (left.GetType() == right.GetType()) return left.Equals(right);
            return (left as string ?? left.ToString()).Equals(right as string ?? right.ToString());
        }
    }

    internal class LooseNotEqualsOperator : BinaryOperatorBase // ~=
    {
        internal LooseNotEqualsOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();

            if (left.GetType() == right.GetType()) return !left.Equals(right);
            return !(left as string ?? left.ToString()).Equals(right as string ?? right.ToString());
        }
    }

    internal class LikeOperator : BinaryOperatorBase // like
    {
        internal LikeOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();
            return Microsoft.VisualBasic.CompilerServices.LikeOperator.LikeString(left as string ?? left.ToString(), right as string ?? right.ToString(), Microsoft.VisualBasic.CompareMethod.Text);
        }
    }

    internal class MatchOperator : BinaryOperatorBase // like
    {
        internal MatchOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();
            return Regex.Match(left as string ?? left.ToString(), right as string ?? right.ToString());
        }
    }

    internal class LessThanOperator : BinaryOperatorBase
    {
        internal LessThanOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();

            if (left.GetType() == right.GetType() && left is IComparable lCom && right is IComparable rCom) return lCom.CompareTo(rCom) < 0;
            return (left as string ?? left.ToString()).CompareTo((right as string ?? right.ToString())) < 0;
        }
    }

    internal class LessThanOrEqualOperator : BinaryOperatorBase
    {
        internal LessThanOrEqualOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();

            if (left.GetType() == right.GetType() && left is IComparable lCom && right is IComparable rCom) return lCom.CompareTo(rCom) <= 0;
            return (left as string ?? left.ToString()).CompareTo((right as string ?? right.ToString())) <= 0;
        }
    }

    internal class GreaterThanOperator : BinaryOperatorBase
    {
        internal GreaterThanOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();

            if (left.GetType() == right.GetType() && left is IComparable lCom && right is IComparable rCom) return lCom.CompareTo(rCom) > 0;
            return (left as string ?? left.ToString()).CompareTo((right as string ?? right.ToString())) > 0;
        }
    }

    internal class GreaterThanOrEqualOperator : BinaryOperatorBase
    {
        internal GreaterThanOrEqualOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            var left = LeftOperand.Eval();
            var right = RightOperand.Eval();

            if (left.GetType() == right.GetType() && left is IComparable lCom && right is IComparable rCom) return lCom.CompareTo(rCom) >= 0;
            return (left as string ?? left.ToString()).CompareTo((right as string ?? right.ToString())) >= 0;
        }
    }

    internal class AndOperator : BinaryOperatorBase
    {
        internal AndOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            return LeftOperand.EvalAsBool() && RightOperand.EvalAsBool();
        }
    }

    internal class OrOperator : BinaryOperatorBase
    {
        internal OrOperator(ExpTreeNode parent) : base(parent)
        {
            //
        }

        internal override object Eval()
        {
            return LeftOperand.EvalAsBool() || RightOperand.EvalAsBool();
        }
    }
}
