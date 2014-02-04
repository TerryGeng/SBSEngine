using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using SBSEngine.Runtime;

namespace SBSEngine.Runtime.Binder
{
    public static class BinaryOperationDelegate
    {
        public static T AddOperation<T>(object left, object right)
        {
            if (BothNumeric(left, right))
            {
                if (left.GetType() == right.GetType())
                {
                    if(left.GetType() == typeof(int))
                        return (T)(Object)new Func<CallSite, object, object, SBSOperator, object>(IntAdd);
                    else if(left.GetType() == typeof(double))
                        return (T)(Object)new Func<CallSite, object, object, SBSOperator, object>(DoubleAdd);
                }
                else
                {
                    if (ConvertBinaryOperationDelegate.IsLeftBiggerNumericType(left,right))
                    {
                        return ConvertBinaryOperationDelegate.MakeRightConvertedDelegate<T>(
                            left.GetType(),
                            right.GetType(), 
                            new Func<object ,object>(ConvertBinaryOperationDelegate.IntToDouble), 
                            new Func<object, object, object>(DoubleAddMethod)
                            );
                    }
                    else
                    {
                        return ConvertBinaryOperationDelegate.MakeLeftConvertedDelegate<T>(
                            left.GetType(),
                            right.GetType(),
                            new Func<object, object>(ConvertBinaryOperationDelegate.IntToDouble),
                            new Func<object, object, object>(DoubleAddMethod)
                            );
                    }
                }
            }

            return default(T);
        }

        public static bool BothNumeric(object x, object y)
        {
            if (IsNumeric(x) && IsNumeric(y))
                return true;
            return false;
        }

        public static bool IsNumeric(object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Int32:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }


        public static object IntAdd(CallSite site, object left, object right, SBSOperator op)
        {
            if (left.GetType() == typeof(int) && right.GetType() == typeof(int))
            {
                return (int)left + (int)right;
            }

            return Update(site, left, right, op);
        }

        public static object DoubleAdd(CallSite site, object left, object right, SBSOperator op)
        {
            if (left.GetType() == typeof(double) && right.GetType() == typeof(double))
            {
                return (double)left + (double)right;
            }

            return Update(site, left, right, op);
        }

        public static object IntAddDouble(CallSite site, object left, object right, SBSOperator op)
        {
            if (left.GetType() == typeof(int) && right.GetType() == typeof(double))
            {
                return (int)left + (double)right;
            }

            return Update(site, left, right, op);
        }

        public static object DoubleAddInt(CallSite site, object left, object right, SBSOperator op)
        {
            if (left.GetType() == typeof(double) && right.GetType() == typeof(int))
            {
                return (double)left + (int)right;
            }

            return Update(site, left, right, op);
        }


        public static object IntAddMethod(object left, object right)
        {
            return (int)left + (int)right;
        }

        public static object DoubleAddMethod(object left, object right)
        {
            return (double)left + (double)right;
        }

        // TODO: Remove this.
        public static object HitOrUpdate(CallSite site, object left, object right, Type leftType, Type rightType , SBSOperator op , Func<object,object,object> method)
        {
            if (left.GetType() == leftType && right.GetType() == rightType)
                return method(right,left);
            return ((CallSite<Func<CallSite, object, object, SBSOperator, object>>)site).Update(site, left, right, op);
        }

        public static bool IfHitChecker(object left,object right,Type leftType,Type rightType)
        {
            if (left.GetType() == leftType && right.GetType() == rightType)
                return true;
            return false;
        }

        public static object Update(CallSite site, object left, object right, SBSOperator op)
        {
            return ((CallSite<Func<CallSite, object, object, SBSOperator, object>>)site).Update(site, left, right, op);
        }

    }
}
