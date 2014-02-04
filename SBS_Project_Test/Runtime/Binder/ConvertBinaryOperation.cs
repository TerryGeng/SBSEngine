using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Runtime.CompilerServices;
using SBSEngine.Runtime;
using System.Reflection;

namespace SBSEngine.Runtime.Binder
{
    public static class ConvertBinaryOperationDelegate
    {
        public static T MakeLeftConvertedDelegate<T>(Type leftType, Type rightType, Func<object, object> convertDele, Func<object, object, object> method)
        {
            ParameterExpression site = Expression.Parameter(typeof(CallSite),"site");
            ParameterExpression left = Expression.Parameter(typeof(object),"left");
            ParameterExpression right = Expression.Parameter(typeof(object),"right");
            ParameterExpression op = Expression.Parameter(typeof(SBSOperator),"op");

            ParameterExpression result = Expression.Parameter(typeof(object));


            MethodInfo ifHit = new Func<object, object, Type, Type, bool>(BinaryOperationDelegate.IfHitChecker).Method;
            MethodInfo update = new Func<CallSite, object, object, SBSOperator, object>(BinaryOperationDelegate.Update).Method;

            Expression code = Expression.Block(
                new[] { result },
                Expression.IfThenElse(
                    Expression.Call(ifHit, left, right, Expression.Constant(leftType), Expression.Constant(rightType)),
                    Expression.Assign(result,
                        Expression.Call(
                            method.Method,
                            Expression.Call(convertDele.Method, left),
                            right
                            )
                        ),
                    Expression.Assign(result, Expression.Call(update, site, left, right, op))
                    ),
                    result
                );

            return (T)(object)Expression.Lambda<Func<CallSite, object, object, SBSOperator, object>>(code, site, left, right, op).Compile();
        }

        public static T MakeRightConvertedDelegate<T>(Type leftType, Type rightType, Func<object, object> convertDele, Func<object, object, object> method)
        {
            ParameterExpression site = Expression.Parameter(typeof(CallSite));
            ParameterExpression left = Expression.Parameter(typeof(object));
            ParameterExpression right = Expression.Parameter(typeof(object));
            ParameterExpression op = Expression.Parameter(typeof(SBSOperator));

            ParameterExpression result = Expression.Parameter(typeof(object));

            MethodInfo ifHit = new Func<object, object, Type, Type, bool>(BinaryOperationDelegate.IfHitChecker).Method;
            MethodInfo update = new Func<CallSite, object, object, SBSOperator, object>(BinaryOperationDelegate.Update).Method;

            Expression code = Expression.Block(
                new[] { result },
                Expression.IfThenElse(
                    Expression.Call(ifHit, left, right, Expression.Constant(leftType), Expression.Constant(rightType)),
                    Expression.Assign(result,
                        Expression.Call(
                            method.Method,
                            left,
                            Expression.Call(convertDele.Method, right)
                            )
                        ),
                    Expression.Assign(result, Expression.Call(update, site, left, right, op))
                    ),
                    result
                );

            return (T)(object)Expression.Lambda<Func<CallSite, object, object, SBSOperator, object>>(code, site, left, right, op).Compile();
        }


        public static bool IsLeftBiggerNumericType(object x, object y)
        {
            if (x.GetType() == typeof(double))
                return true;
            return false;
        }

        public static object IntToDouble(object x)
        {
            return (double)(int)x;
        }
    }
}
