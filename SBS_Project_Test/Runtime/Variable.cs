using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SBSEngine.Runtime
{
    class Variable
    {
        public string Name { get; private set; }
        public Type Type { get; private set; }
        public ParameterExpression Expr { get; private set; }

        public Variable(string name,Type type)
        {
            Name = name;
            Type = type;
            Expr = Expression.Parameter(type,name);
        }
        
    }
}
