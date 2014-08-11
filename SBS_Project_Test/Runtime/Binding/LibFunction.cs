using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Linq.Expressions;

namespace SBSEnvironment.Runtime
{
    class LibFunction : IFunction
    {
        public string Name { get; private set; }
        public int ArgCount { get; private set; }

        private Func<object[], object> FuncDelegate;

        public LibFunction(string name, int argCount ,Func<object[], object> funcDelegate)
        {
            this.Name = name;
            this.FuncDelegate = funcDelegate;
            this.ArgCount = argCount;
        }

        public Expression GetInvokeExpr(ParameterExpression argsList)
        {
            return Expression.Call(FuncDelegate.Method, argsList);
        }

        public object Emit(object[] args)
        {
            return FuncDelegate(args);
        }
    }
}
