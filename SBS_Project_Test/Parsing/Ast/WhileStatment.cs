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
    class WhileStatment : SBSAst
    {
        private MSAst.Expression condition;
        private MSAst.Expression body;
        private MSAst.LabelTarget breakLabel;
        private MSAst.LabelTarget continueLabel;

        public WhileStatment(MSAst.Expression condition, MSAst.Expression body, MSAst.LabelTarget breakLabel, MSAst.LabelTarget continueLabel)
        {
            this.condition = condition;
            this.body = body;
            this.breakLabel = breakLabel;
            this.continueLabel = continueLabel;
        }

        public override MSAst.Expression Reduce()
        {
            return  MSAst.Expression.Loop(
                    MSAst.Expression.IfThenElse(
                        MSAst.Expression.Convert(condition.Reduce(), typeof(bool)),
                        body.Reduce(),
                        MSAst.Expression.Break(breakLabel)), 
                    breakLabel,
                    continueLabel
                    );
        }
    }
}
