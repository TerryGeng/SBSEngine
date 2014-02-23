using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBSEngine.Runtime
{
    public enum SBSOperator
    {
        Null,

        Add,                // '+'
        Subtract,           // '-'
        Multiply,           // '*'
        Divide,             // '/'

        Assign,             // '='
        AddAssign,          // '+='

        GreaterThan,        // '>'
        LessThan,           // '<'
        GreaterThanOrEqual, // '>='
        LessThanOrEqual,    // '<='
        Equal,              // '=='
        NotEqual,           // '<>'

        And,                // 'AND'
        Or                  // 'OR'
    }

    /*
     * Precedence(From high to low):
     * '*' '/'
     * '+' '/'
     * '>' '<' '>=' '<='
     * '==' '<>'
     * 'AND'
     * 'OR'
     * '=' '+='
     */
}
