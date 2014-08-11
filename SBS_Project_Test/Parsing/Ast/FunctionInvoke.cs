using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAst = System.Linq.Expressions;
using SBSEnvironment.Runtime;
using SBSEnvironment.Parsing;

namespace SBSEnvironment.Parsing.Ast
{
    class FunctionInvoke : SBSAst
    {
        private string name;
        private List<MSAst.Expression> argsArray;
        private ParsingContext context;

        public FunctionInvoke(string name, List<MSAst.Expression> argsArray, ParsingContext context)
        {
            this.name = name;
            this.argsArray = argsArray;
            this.context = context;
        }

        public override MSAst.Expression Reduce()
        {
            if (argsArray != null)
            {
                for (int i = 0; i < argsArray.Count; ++i)
                {
                    argsArray[i] = Expression.Convert(argsArray[i].Reduce(), typeof(object));
                }

                MSAst.Expression args = Expression.NewArrayInit(typeof(object), argsArray);
                MSAst.Expression body = Expression.Dynamic(
                    context.FunctionBinder,
                    typeof(object),
                    Expression.Constant(name),
                    args
                    );

                return body;
            }

            return Expression.Dynamic(
                    context.FunctionBinder,
                    typeof(object),
                    Expression.Constant(name),
                    Expression.Constant(null)
                    );


        }
    }
}
