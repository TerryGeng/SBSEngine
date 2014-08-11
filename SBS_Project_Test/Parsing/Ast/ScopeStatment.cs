using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using MSAst = System.Linq.Expressions;

namespace SBSEnvironment.Parsing.Ast
{
    class ScopeStatment : SBSAst
    {
        public Scope LocalScope;

        private IEnumerable<MSAst.Expression> statments;

        public ScopeStatment(IEnumerable<MSAst.Expression> _statments, Scope scope)
        {
            statments = _statments;
            LocalScope = scope;
        }

        public ScopeStatment(IEnumerable<MSAst.Expression> _statments){
            LocalScope = null;
            statments = _statments;
        }

        public override MSAst.Expression Reduce()
        {
            if (statments == null || statments.Count<object>() == 0) MSAst.Expression.Constant(null);

            var list = new LinkedList<MSAst.Expression>();
            var iterator = statments.GetEnumerator();

            foreach (MSAst.Expression stmt in statments)
            {
                list.AddLast(stmt.Reduce());
            }

            return MSAst.Expression.Block(LocalScope.LocalVariables,list);
        }
    }
}
