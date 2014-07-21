using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAst = System.Linq.Expressions;
using SBSEngine.Runtime;
using SBSEngine.Parsing;

namespace SBSEngine.Parsing.Ast
{
    abstract class SBSAst : MSAst.Expression
    {
        public override MSAst.ExpressionType NodeType
        {
            get
            {
                return MSAst.ExpressionType.Extension;
            }
        }

        public override Type Type
        {
            get
            {
                return base.Type;
            }
        }
    }

    abstract class Expression : SBSAst
    {
        public override Type Type
        {
            get
            {
                return typeof(object);
            }
        }
    }
}
