using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAst = System.Linq.Expressions;
using SBSEnvironment.Parsing;
using SBSEnvironment.Parsing.Ast;

namespace SBSEnvironment.Runtime
{
    class Function : IFunction
    {
        public string Name { get; private set; }
        public int ArgCount { get; private set; }

        public IList<SBSVariable> Arguments;
        public Func<object[], object> EmitDelegate
        {
            get
            {
                if (funcDelegate == null)
                    funcDelegate = Compile();

                return funcDelegate;
            }
        }

        private ScopeStatment funcCode;
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
                funcDelegate = Compile();

            return funcDelegate.Invoke(args);
        }

        private Func<object[], object> Compile()
        {
            var args = funcCode.LocalScope.GetVariableExpr("@{args}") as MSAst.ParameterExpression;

            return MSAst.Expression.Lambda<Func<object[], object>>(funcCode.Reduce(), new[] { args }).Compile();
        }
    }
}
