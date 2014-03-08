using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace SBSEngine.Runtime.Binding.Sorter
{

    using ISorterDictionary = IDictionary<Tuple<Type, SBSOperator>, SorterDelegate>;
    using SorterDictionary = Dictionary<Tuple<Type, SBSOperator>, SorterDelegate>;
    using BindDelegate = Func<CallSite, object, object, SBSOperator,object>;
    using DelegateDictionary = Dictionary<Tuple<Type, Type, SBSOperator>, /*BindDelegate*/Func<CallSite, object, object, SBSOperator, object>>;
    using System.Reflection;

    delegate Expression SorterDelegate(Type leftType, Type rightType, SBSOperator op, Expression left, Expression right, ParameterExpression result);

    class BinaryOpSorter
    {
        public ISorterDictionary Sorters;
        public DelegateDictionary DelegateBuffer;

        MethodInfo update;

        public BinaryOpSorter()
        {
            Sorters = new SorterDictionary();
            DelegateBuffer = new DelegateDictionary();
            update = typeof(BinaryOpSorter).GetMethod("Update");
        }

        public BinaryOpSorter(ISorterDictionary sorters)
        {
            Sorters = sorters;
            DelegateBuffer = new DelegateDictionary();
            update = typeof(BinaryOpSorter).GetMethod("Update");
        }

        public T GetBinaryDelegate<T>(Type leftType, Type rightType, SBSOperator op)
        {
            BindDelegate compiled;
            Tuple<Type, Type, SBSOperator> bufferKey = new Tuple<Type, Type, SBSOperator>(leftType, rightType, op);
            if (DelegateBuffer.TryGetValue(bufferKey, out compiled))
            {
                return (T)(object)compiled;
            }

            SorterDelegate Sorter;
            if (!Sorters.TryGetValue(new Tuple<Type, SBSOperator>(leftType, op), out Sorter))
            {
                Debug.Assert(false, "Unknown operation."); // TODO: Error process.
                return default(T);
            }

            ParameterExpression site = Expression.Parameter(typeof(CallSite), "site");
            ParameterExpression left = Expression.Parameter(typeof(object), "left");
            ParameterExpression right = Expression.Parameter(typeof(object), "right");
            ParameterExpression opParam = Expression.Parameter(typeof(SBSOperator), "op");
            ParameterExpression result = Expression.Parameter(typeof(object));

            Expression body = Expression.Block(
                    new[] { result },
                    Expression.IfThenElse(GetTypeChecker(leftType, rightType, left, right),
                    Sorter(leftType, rightType, op, left, right, result),
                    Expression.Assign(result, Expression.Call(update, site, left, right, opParam))
                    ),
                    result
                );

            compiled = Expression.Lambda<BindDelegate>(body, site, left, right, opParam).Compile();

            DelegateBuffer.Add(bufferKey, compiled);

            return (T)(object)(compiled);

        }

        private Expression GetTypeChecker(Type leftType, Type rightType, ParameterExpression left, ParameterExpression right)
        {
            return Expression.AndAlso(
                Expression.TypeIs(left,leftType),
                Expression.TypeIs(right,rightType)
                );
        }

        public void RegisterOperation(Type leftType, SBSOperator op, SorterDelegate dele)
        {
            Sorters.Add(new Tuple<Type, SBSOperator>(leftType, op), dele);
        }

        public static object Update(CallSite site, object left, object right, SBSOperator op)
        {
            return ((CallSite<Func<CallSite, object, object, SBSOperator, object>>)site).Update(site, left, right, op);
        }
    }
}
