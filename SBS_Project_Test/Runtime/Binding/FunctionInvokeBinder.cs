using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using SBSEnvironment.Runtime;
using SBSEnvironment.Runtime.Binding;
using System.Diagnostics;

namespace SBSEnvironment.Runtime.Binding
{
    /// <summary>
    /// This is a binder which provide runtime function invoking binding.
    /// </summary>
    class FunctionInvokeBinder : CallSiteBinder
    {
        private ExecutableUnit unit;

        public FunctionInvokeBinder(ExecutableUnit unit)
        {
            this.unit = unit;
        }

        public override System.Linq.Expressions.Expression Bind(object[] args, System.Collections.ObjectModel.ReadOnlyCollection<System.Linq.Expressions.ParameterExpression> parameters, System.Linq.Expressions.LabelTarget returnLabel)
        {
            return null;
        }

        public override T BindDelegate<T>(CallSite<T> site, object[] args)
        {
            return GetInvokeDelegate<T>(args);
        }

        public T GetInvokeDelegate<T>(object[] args)
        {
            IFunction func;
            string name = (string)args[0];

            if ((func = unit.GetFunction(name)) == null || func.ArgCount != ((object[])args[1]).Length)
            {
                Debug.Assert(false, "Undefined function."); // TODO: Error process.
                return default(T);
            }

            ParameterExpression site = Expression.Parameter(typeof(CallSite), "site");
            ParameterExpression namePara = Expression.Parameter(typeof(string), "name");
            ParameterExpression argsList = Expression.Parameter(typeof(object[]), "argsList");

            Expression body = func.GetInvokeExpr(argsList);

            var compiled = Expression.Lambda<Func<CallSite, string, object[], object>>(body, site, namePara, argsList).Compile();

            return (T)(object)compiled;
        }


    }
}
