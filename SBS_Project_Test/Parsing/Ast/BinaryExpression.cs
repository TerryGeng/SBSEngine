using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAst = System.Linq.Expressions;
using SBSEnvironment.Runtime;
using SBSEnvironment.Parsing;

namespace SBSEnvironment.Parsing.Ast
{
    class BinaryExpression : Expression
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
            this._context = _context;
        }

        public override MSAst.Expression Reduce()
        {
            if (left == null || right == null)
                return left ?? right;

            return MSAst.Expression.Dynamic(
                _context.BinaryBinder,
                typeof(object),
                MSAst.Expression.Convert(left.Reduce(),typeof(object)),
                MSAst.Expression.Convert(right.Reduce(), typeof(object)),
                MSAst.Expression.Constant(_operator)
                );
        }
    }
}
