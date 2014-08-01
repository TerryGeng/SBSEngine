using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using MSAst = System.Linq.Expressions;

namespace SBSEnvironment.Parsing
{
    class SBSVariable
    {
        public string Name { get; private set; }
        public MSAst.Expression Expr { get; private set; }

        public SBSVariable(string name)
        {
            Name = name;
            Expr = Expression.Parameter(typeof(object), name);
        }

        public SBSVariable(string name, MSAst.Expression expr)
        {
            Name = name;
            Expr = expr;
        }
        
    }
}
