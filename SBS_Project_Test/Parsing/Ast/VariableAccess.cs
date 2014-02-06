using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAst = System.Linq.Expressions;
using SBSEngine.Runtime;
using SBSEngine.Parsing;

namespace SBSEngine.Parsing.Ast
{
    class VariableAccess : MSAst.Expression
    {
        private string _name;
        private string _sub;
        private AccessMethod _method;
        private ParsingContext _context;
        private Scope _scope;

        public AccessMethod Access
        {
            get { return _method; }
            set { _method = value; }
        }

        public VariableAccess(string name, ParsingContext context, Scope scope, AccessMethod method)
        {
            _name = name;
            _context = context;
            _scope = scope;
            _method = method;
        }

        public override MSAst.Expression Reduce()
        {
            switch (_method)
            {
                case AccessMethod.Get:
                    return _scope.GetVariableExpr(_name);
                case AccessMethod.GetOrMake:
                default:
                    return _scope.GetOrMakeVariableExpr(_name);
            }
        }

        public MSAst.Expression Assign(MSAst.Expression value)
        {
            return MSAst.Expression.Assign(this.Reduce(), MSAst.Expression.Convert(value.Reduce(),typeof(object)));
        }

        public enum AccessMethod
        {
            Null,

            Get,
            GetOrMake
        }
    }
}
