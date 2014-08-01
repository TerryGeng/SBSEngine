using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAst = System.Linq.Expressions;
using SBSEnvironment.Parsing;
using SBSEnvironment.Parsing.Ast;

namespace SBSEnvironment.Runtime
{
    class ExecutableUnit
    {
        private Dictionary<string, IFunction> functionDict;

        public void AddFunction(IFunction func)
        {
            functionDict.Add(func.Name, func);
        }
    }
}
