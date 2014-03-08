using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace SBSEngine.Runtime.Binding.Sorter
{
    using SorterDelegate = Func<Type, Type, SBSOperator, ParameterExpression, ParameterExpression, Expression>;
    using System.Diagnostics;

    static class NumericOpSorter
    {
        public static void SelfRegister(BinaryOpSorter sorter)
        {
            sorter.RegisterOperation(typeof(int), SBSOperator.Add, NumericAdd);
            sorter.RegisterOperation(typeof(double), SBSOperator.Add, NumericAdd);
        }

        public static Expression NumericAdd(Type leftType, Type rightType, SBSOperator op, Expression left, Expression right, ParameterExpression result)
        {
            Type leftTarget, rightTarget;
            GetConvertTarget(leftType, rightType, out leftTarget, out rightTarget);

            left = GetConvert(leftType, leftTarget, left);
            right = GetConvert(rightType, rightTarget, right);

            return Expression.Block(Expression.Assign(result, Expression.Convert(Expression.Add(left, right), typeof(object))));
        }

        public static Expression GetConvert(Type type, Type targetType, Expression param)
        {
            if (type == targetType)
                return Expression.Convert(param, type);

            return Expression.Convert(Expression.Convert(param, type), targetType);
        }

        public static void GetConvertTarget(Type leftType, Type rightType, out Type targetLeft, out Type targetRight)
        {
            if (leftType == rightType)
            {
                targetLeft = leftType;
                targetRight = rightType;
                return;
            }
            else if (rightType == typeof(double))
            {
                targetLeft = typeof(double);
                targetRight = rightType;
                return;
            }
            else if (leftType == typeof(double))
            {
                targetRight = typeof(double);
                targetLeft = leftType;
                return;
            }

            Debug.Assert(false,"Unknown numeric type.");
            targetLeft = null;
            targetRight = null;
        }
    }
}
