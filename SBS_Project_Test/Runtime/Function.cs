using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAst = System.Linq.Expressions;
using SBSEnvironment.Parsing;
using SBSEnvironment.Parsing.Ast;
using System.Diagnostics;

namespace SBSEnvironment.Runtime
{
    class Function : IFunction
    {
        public string Name { get; private set; }
        public int ArgCount { get; private set; }

        public IList<SBSVariable> Arguments;
        public MSAst.Expression<Func<object[], object>> Lambda
        {
            get
            {
                if (lambda == null)
                    lambda = GetLambda();

                return lambda;
            }
        }

        private ScopeStatment funcCode;
        private MSAst.Expression<Func<object[], object>> lambda;
        private Func<object[], object> funcDelegate;

        public Function(string name, IList<SBSVariable> args, ScopeStatment funcCode)
        {
            this.Name = name;
            this.Arguments = args;
            this.funcCode = funcCode;

            ArgCount = args.Count;
        }

        public object Emit(object[] args)
        {
            if (funcDelegate == null)
                funcDelegate = Lambda.Compile();

            return funcDelegate(args);
        }

        private MSAst.Expression<Func<object[], object>> GetLambda()
        {
            var args = funcCode.LocalScope.GetVariableExpr("@{args}") as MSAst.ParameterExpression;

            return MSAst.Expression.Lambda<Func<object[], object>>(funcCode.Reduce(), new[] { args });
        }

        public MSAst.Expression GetInvokeExpr(MSAst.ParameterExpression argsList)
        {
            var args = funcCode.LocalScope.GetVariableExpr("@{args}") as MSAst.ParameterExpression;

            //MSAst.BlockExpression code = funcCode.Reduce() as MSAst.BlockExpression;
            //MSAst.Expression body = MSAst.Expression.Block(
            //    new[] { args },
            //    MSAst.Expression.Assign(args , argsList),
            //    code
            //    );
            //return body;

            return Expression.Invoke(Lambda, argsList);
        }
    }
}
