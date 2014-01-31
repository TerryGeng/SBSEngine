using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAst = System.Linq.Expressions;
using SBSEngine.Runtime;
using SBSEngine.Parsing;

namespace SBSEngine.Parsing.Ast
{
    class BinaryExpression : MSAst.Expression
    {
        private MSAst.Expression left;
        private MSAst.Expression right;
        private SBSOperator _operator;
        private ParsingContext _context;

        public BinaryExpression(MSAst.Expression _left,MSAst.Expression _right,SBSOperator _operator,ParsingContext _context)
        {
            left = _left;
            right = _right;
            this._operator = _operator;
        }

        public override MSAst.Expression Reduce()
        {
            return MSAst.Expression.Dynamic(
                _context.BinaryBinder,
                typeof(object),
                left.Reduce(),
                right.Reduce()
                );
        }
    }
}
