using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBSEngine.Parsing.Ast
{
    public enum SBSOperator
    {
        Undefined,

        Add,
        Subtract,
        Multiply,
        Divide,

        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Equal
    }
}
