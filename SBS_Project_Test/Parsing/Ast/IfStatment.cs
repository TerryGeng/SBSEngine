using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAst = System.Linq.Expressions;
using SBSEnvironment.Runtime;
using SBSEnvironment.Parsing;
using System.Diagnostics;

namespace SBSEnvironment.Parsing.Ast
{
    class IfStatment : SBSAst
    {
        private MSAst.Expression _condition;
        private MSAst.Expression _then;
        private MSAst.Expression _else;

        public IfStatment(MSAst.Expression _condition, MSAst.Expression _then, MSAst.Expression _else = null)
        {
            this._condition = _condition;
            this._then = _then;
            this._else = _else;
        }

        public override MSAst.Expression Reduce()
        {
            if(_else != null)
                return MSAst.Expression.IfThenElse(MSAst.Expression.Convert(_condition, typeof(bool)), _then.Reduce(), _else.Reduce());
            else
                return MSAst.Expression.IfThen(MSAst.Expression.Convert(_condition, typeof(bool)), _then.Reduce());
        }
    }
}
