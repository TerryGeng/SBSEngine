using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Linq.Expressions;

namespace SBSEnvironment.Runtime
{
    interface IFunction
    {
        string Name { get; }
        int ArgCount { get; }
        Expression GetInvokeExpr(ParameterExpression argsList);
        object Emit(object[] args);
    }
}
