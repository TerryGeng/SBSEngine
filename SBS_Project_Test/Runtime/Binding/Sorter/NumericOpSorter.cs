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
            sorter.RegisterOperation(typeof(int), SBSOperator.Subtract, NumericSub);
            sorter.RegisterOperation(typeof(double), SBSOperator.Subtract, NumericSub);
            sorter.RegisterOperation(typeof(int), SBSOperator.Multiply, NumericMul);
            sorter.RegisterOperation(typeof(double), SBSOperator.Multiply, NumericMul);
            sorter.RegisterOperation(typeof(int), SBSOperator.Divide, NumericDiv);
            sorter.RegisterOperation(typeof(double), SBSOperator.Divide, NumericDiv);
            sorter.RegisterOperation(typeof(int), SBSOperator.Equal, NumericEqual);
            sorter.RegisterOperation(typeof(double), SBSOperator.Equal, NumericEqual);
            sorter.RegisterOperation(typeof(int), SBSOperator.GreaterThan, NumericGreater);
            sorter.RegisterOperation(typeof(double), SBSOperator.GreaterThan, NumericGreater);
            sorter.RegisterOperation(typeof(int), SBSOperator.LessThan, NumericLess);
            sorter.RegisterOperation(typeof(double), SBSOperator.LessThan, NumericLess);
            sorter.RegisterOperation(typeof(int), SBSOperator.GreaterThanOrEqual, NumericGreaterEqual);
            sorter.RegisterOperation(typeof(double), SBSOperator.GreaterThanOrEqual, NumericGreaterEqual);
            sorter.RegisterOperation(typeof(int), SBSOperator.LessThanOrEqual, NumericLessEqual);
            sorter.RegisterOperation(typeof(double), SBSOperator.LessThanOrEqual, NumericLessEqual);
            sorter.RegisterOperation(typeof(int), SBSOperator.NotEqual, NumericNotEqual);
            sorter.RegisterOperation(typeof(double), SBSOperator.NotEqual, NumericNotEqual);
        }

        #region Operation Delegate Generators
        #region Arithmetic Operations
        public static Expression NumericAdd(Type leftType, Type rightType, SBSOperator op, Expression left, Expression right, ParameterExpression result)
        {
            GetProcessedLeftRight(leftType, rightType, ref left, ref right);
            return Expression.Block(Expression.Assign(result, Expression.Convert(Expression.Add(left, right), typeof(object))));
        }

        public static Expression NumericSub(Type leftType, Type rightType, SBSOperator op, Expression left, Expression right, ParameterExpression result)
        {
            GetProcessedLeftRight(leftType, rightType, ref left, ref right);
            return Expression.Block(Expression.Assign(result, Expression.Convert(Expression.Subtract(left, right), typeof(object))));
        }

        public static Expression NumericMul(Type leftType, Type rightType, SBSOperator op, Expression left, Expression right, ParameterExpression result)
        {
            GetProcessedLeftRight(leftType, rightType, ref left, ref right);
            return Expression.Block(Expression.Assign(result, Expression.Convert(Expression.Multiply(left, right), typeof(object))));
        }

        public static Expression NumericDiv(Type leftType, Type rightType, SBSOperator op, Expression left, Expression right, ParameterExpression result)
        {
            left = GetConvert(leftType, typeof(double), left);
            right = GetConvert(rightType, typeof(double), right);

            return Expression.Block(Expression.Assign(result, Expression.Convert(Expression.Divide(left, right), typeof(object))));
        }
        #endregion 
        #region Comparison Operations
        public static Expression NumericEqual(Type leftType, Type rightType, SBSOperator op, Expression left, Expression right, ParameterExpression result)
        {
            GetProcessedLeftRight(leftType, rightType, ref left, ref right);
            return Expression.Block(Expression.Assign(result, Expression.Convert(Expression.Equal(left, right), typeof(object))));
        }

        public static Expression NumericNotEqual(Type leftType, Type rightType, SBSOperator op, Expression left, Expression right, ParameterExpression result)
        {
            GetProcessedLeftRight(leftType, rightType, ref left, ref right);
            return Expression.Block(Expression.Assign(result, Expression.Convert(Expression.NotEqual(left, right), typeof(object))));
        }

        public static Expression NumericGreater(Type leftType, Type rightType, SBSOperator op, Expression left, Expression right, ParameterExpression result)
        {
            GetProcessedLeftRight(leftType, rightType, ref left, ref right);
            return Expression.Block(Expression.Assign(result, Expression.Convert(Expression.GreaterThan(left, right), typeof(object))));
        }

        public static Expression NumericLess(Type leftType, Type rightType, SBSOperator op, Expression left, Expression right, ParameterExpression result)
        {
            GetProcessedLeftRight(leftType, rightType, ref left, ref right);
            return Expression.Block(Expression.Assign(result, Expression.Convert(Expression.LessThan(left, right), typeof(object))));
        }

        public static Expression NumericGreaterEqual(Type leftType, Type rightType, SBSOperator op, Expression left, Expression right, ParameterExpression result)
        {
            GetProcessedLeftRight(leftType, rightType, ref left, ref right);
            return Expression.Block(Expression.Assign(result, Expression.Convert(Expression.GreaterThanOrEqual(left, right), typeof(object))));
        }

        public static Expression NumericLessEqual(Type leftType, Type rightType, SBSOperator op, Expression left, Expression right, ParameterExpression result)
        {
            GetProcessedLeftRight(leftType, rightType, ref left, ref right);
            return Expression.Block(Expression.Assign(result, Expression.Convert(Expression.LessThanOrEqual(left, right), typeof(object))));
        }
        #endregion

        public static void GetProcessedLeftRight(Type leftType, Type rightType,ref Expression left, ref Expression right)
        {
            Type leftTarget, rightTarget;
            GetConvertTarget(leftType, rightType, out leftTarget, out rightTarget);

            left = GetConvert(leftType, leftTarget, left);
            right = GetConvert(rightType, rightTarget, right);
        }

        #endregion

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
