﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using SBSEngine.Runtime;
using MSAst = System.Linq.Expressions;
using SBSEngine.Parsing.Ast;

namespace SBSEngine.Parsing
{
    class Scope  // TODO: Move this to ScopeStatment
    {
        private Scope parentScope;
        private Dictionary<string, Variable> localVarsDict;
        private List<MSAst.ParameterExpression> localVars;

        private LabelTarget breakLabel;
        private LabelTarget continueLabel;

        public LabelTarget BreakLabel
        {
            get
            {
                if (breakLabel != null) return breakLabel;
                else if (parentScope != null) return parentScope.BreakLabel;

                return null;
            }

            set { breakLabel = value; }
        }

        public LabelTarget ContinueLabel
        {
            get
            {
                if (continueLabel != null) return continueLabel;
                else if (parentScope != null) return parentScope.ContinueLabel;

                return null;
            }

            set { continueLabel = value; }
        }


        public List<MSAst.ParameterExpression> LocalVariables
        {
            get
            {
                return localVars;
            }
        }

        public Scope()
        {
            localVarsDict = new Dictionary<string, Variable>(10);
            localVars = new List<MSAst.ParameterExpression>(10);
            parentScope = null;
        }

        public Scope(Scope parent)
        {
            parentScope = parent;
            localVarsDict = new Dictionary<string, Variable>(10);
            localVars = new List<MSAst.ParameterExpression>(10);
        }

        /// <summary>
        /// Get or make a variable's parameter expression by it's name.
        /// </summary>
        public ParameterExpression GetOrMakeVariableExpr(string name)
        {
            return GetVariableExpr(name) ?? MakeVaribaleExpr(name);
        }

        /// <summary>
        /// Get a variable's parameter expression by it's name.
        /// If specific variable isn't exist in this scope, it will look up outer scope, if this
        /// is already the outest scope, return null.
        /// </summary>
        public ParameterExpression GetVariableExpr(string name)
        {
            if (localVarsDict.ContainsKey(name))
                return localVarsDict[name].Expr;

            if (parentScope != null)
                return parentScope.GetVariableExpr(name);

            return null;
        }

        /// <summary>
        /// Make a variable in this scope by it's name and return it's parameter expression
        /// </summary>
        public ParameterExpression MakeVaribaleExpr(string name)
        {
            Variable variable = new Variable(name, typeof(object));
            localVars.Add(variable.Expr);
            localVarsDict.Add(name, variable);

            return variable.Expr;
        }
    }
}
