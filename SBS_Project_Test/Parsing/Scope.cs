using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using SBSEngine.Runtime;

namespace SBSEngine.Parsing
{
    class Scope
    {
        private Dictionary<string, Variable> variables;
        private List<Variable> localDefinedVariables;

        public Dictionary<string, Variable> Variables
        {
            get
            {
                return variables;
            }
        }

        public Scope()
        {
            variables = new Dictionary<string, Variable>(10);
            localDefinedVariables = new List<Variable>(10);
        }

        public Scope(Dictionary<string,Variable> originalVariables)
        {
            variables = new Dictionary<string, Variable>(originalVariables);
            localDefinedVariables = new List<Variable>(10);
        }

        /// <summary>
        /// Get a variable's parameter expression by it's name and it's type.
        /// </summary>
        public ParameterExpression GetVariableExpr(string name, Type type)
        {
            if (variables.ContainsKey(name))
            {
                Variable variable = variables[name];

                if (variable.Type != type)
                {
                    variable = new Variable(name, type);
                    variables[name] = variable;
                    localDefinedVariables.Add(variable);
                }

                return variable.Expr;
            }
            else
            {
                Variable variable = new Variable(name,type);
                localDefinedVariables.Add(variable);
                variables.Add(name, variable);

                return variable.Expr;
            }
        }
    }
}
