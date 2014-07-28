using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAst = System.Linq.Expressions;
using SBSEnvironment.Runtime;
using SBSEnvironment.Parsing;

namespace SBSEnvironment.Parsing.Ast
{
    abstract class SBSAst : MSAst.Expression
    {
        public override bool CanReduce
        {
            get
            {
                return true;
            }
        }

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
                return typeof(void);
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
