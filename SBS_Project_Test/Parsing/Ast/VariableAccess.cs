using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAst = System.Linq.Expressions;
using SBSEngine.Runtime;
using SBSEngine.Parsing;
using System.Diagnostics;

namespace SBSEngine.Parsing.Ast
{
    class VariableAccess : MSAst.Expression
    {
        private string _name;
        private string _sub;
        private ParsingContext _context;
        private Scope _scope;

        public VariableAccess(string name, ParsingContext context, Scope scope)
        {
            _name = name;
            _context = context;
            _scope = scope;
        }

        public override MSAst.Expression Reduce()
        {
            var expr = _scope.GetVariableExpr(_name);
            Debug.Assert(expr != null, "Undefined variable."); // TODO: Add new exception.

            return _scope.GetVariableExpr(_name);
        }

        public MSAst.Expression Assign(MSAst.Expression value)
        {
            return MSAst.Expression.Assign(_scope.GetOrMakeVariableExpr(_name), MSAst.Expression.Convert(value.Reduce(),typeof(object)));
        }

        public MSAst.Expression Assign(MSAst.Expression value, SBSOperator op)
        {
            if(op == SBSOperator.Assign)
                return MSAst.Expression.Assign(_scope.GetOrMakeVariableExpr(_name), MSAst.Expression.Convert(value.Reduce(), typeof(object)));

            var variable = _scope.GetVariableExpr(_name);
            BinaryExpression binary = null;

            switch (op)
            {
                case SBSOperator.AddAssign:
                    binary = new BinaryExpression(variable, value, SBSOperator.Add, _context);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            return MSAst.Expression.Assign(variable, binary.Reduce());
        }
    }
}
