using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAst = System.Linq.Expressions;
using SBSEngine.Runtime;
using SBSEngine.Parsing;

namespace SBSEngine.Parsing.Ast
{
    class AssignExpression : MSAst.Expression
    {
        private VariableAccess _variable;
        private MSAst.Expression _value;

        public AssignExpression(VariableAccess variable, MSAst.Expression value)
        {
            _variable = variable;
            _value = value;
        }

        public override MSAst.Expression Reduce()
        {
            return _variable.Assign(_value);
        }
    }
}
