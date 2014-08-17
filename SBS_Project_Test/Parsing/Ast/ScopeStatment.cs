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

        private LinkedList<MSAst.Expression> statments;

        public ScopeStatment(LinkedList<MSAst.Expression> _statments, Scope scope = null)
        {
            statments = _statments;
            LocalScope = scope;
        }

        public ScopeStatment(Scope scope = null)
        {
            statments = new LinkedList<MSAst.Expression>();
            LocalScope = scope;
        }

        public void AddStatment(MSAst.Expression stmt)
        {
            statments.AddLast(stmt);
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

            return MSAst.Expression.Block(typeof(object), LocalScope.LocalVariables, list);
        }
    }
}
