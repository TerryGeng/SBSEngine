using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAst = System.Linq.Expressions;

namespace SBSEnvironment.Parsing.Ast
{
    class ScopeStatment : SBSAst
    {
        public IEnumerable<MSAst.Expression> Statments;
        public Scope LocalScope;

        public ScopeStatment(IEnumerable<MSAst.Expression> statments, Scope scope)
        {
            Statments = statments;
            LocalScope = scope;
        }

        public ScopeStatment(IEnumerable<MSAst.Expression> statments){
            LocalScope = null;
            Statments = statments;
        }

        public override MSAst.Expression Reduce()
        {
            var list = new LinkedList<MSAst.Expression>();
            var iterator = Statments.GetEnumerator();

            foreach (MSAst.Expression stmt in Statments)
            {
                list.AddLast(stmt.Reduce());
            }

            return MSAst.Expression.Block(LocalScope.LocalVariables,list);
        }
    }
}
