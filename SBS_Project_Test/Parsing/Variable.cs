using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SBSEnvironment.Parsing
{
    class Variable
    {
        public string Name { get; private set; }
        public ParameterExpression Expr { get; private set; }

        public Variable(string name,Type type)
        {
            Name = name;
            Expr = Expression.Parameter(type,name);
        }
        
    }
}
