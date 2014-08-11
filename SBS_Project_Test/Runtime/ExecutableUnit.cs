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

        public ExecutableUnit()
        {
            functionDict = new Dictionary<string, IFunction>();
        }

        public void AddFunction(IFunction func)
        {
            functionDict.Add(func.Name, func);
        }

        public IFunction GetFunction(string name)
        {
            IFunction func = null;
            functionDict.TryGetValue(name, out func);

            return func;
        }

        public object Run()
        {
            return null;
        }
    }
}
