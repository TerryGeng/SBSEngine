using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAst = System.Linq.Expressions;

namespace SBSEngine.Parsing.Ast
{
    class BinaryExpression : MSAst.Expression
    {
        private MSAst.Expression left;
        private MSAst.Expression right;
        private SBSOperator _operator;

        public BinaryExpression(MSAst.Expression _left,MSAst.Expression _right,SBSOperator _operator)
        {
            left = _left;
            right = _right;
            this._operator = _operator;
        }

        public override MSAst.Expression Reduce()
        {
            return base.Reduce(); // TODO
        }
    }
}
