﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using SBSEnvironment.Runtime;
using MSAst = System.Linq.Expressions;
using SBSEnvironment.Parsing.Ast;

namespace SBSEnvironment.Parsing
{
    class Scope
    {
        private Scope parentScope;
        private Dictionary<string, MSAst.Expression> localVarsDict;
        private List<ParameterExpression> localVars;

        private LabelTarget breakLabel;
        private LabelTarget continueLabel;
        private LabelTarget returnLabel;
        private MSAst.ParameterExpression returnVal;

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

        public LabelTarget ReturnLabel
        {
            get
            {
                if (returnLabel != null) return returnLabel;
                else if (parentScope != null) return parentScope.ReturnLabel;

                return null;
            }

            set { returnLabel = value; }
        }

        public MSAst.ParameterExpression ReturnValue
        {
            get
            {
                if (returnVal != null) return returnVal;
                else if (parentScope != null) return parentScope.ReturnValue;

                return null;
            }

            set { returnVal = value; }
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
            localVarsDict = new Dictionary<string, MSAst.Expression>(10);
            localVars = new List<MSAst.ParameterExpression>(10);
            parentScope = null;
        }

        public Scope(Scope parent)
        {
            parentScope = parent;
            localVarsDict = new Dictionary<string, MSAst.Expression>(10);
            localVars = new List<MSAst.ParameterExpression>(10);
        }

        /// <summary>
        /// Get or make a variable's parameter expression by it's name.
        /// </summary>
        public MSAst.Expression GetOrMakeVariableExpr(string name)
        {
            return GetVariableExpr(name) ?? MakeVaribaleExpr(name);
        }

        /// <summary>
        /// Get a variable's parameter expression by it's name.
        /// If specific variable isn't exist in this scope, it will look up outer scope, if this
        /// is already the outest scope, return null.
        /// </summary>
        public MSAst.Expression GetVariableExpr(string name)
        {
            if (localVarsDict.ContainsKey(name))
                return localVarsDict[name];

            if (parentScope != null)
                return parentScope.GetVariableExpr(name);

            return null;
        }

        /// <summary>
        /// Make a variable in this scope by it's name and return it's parameter expression
        /// </summary>
        public ParameterExpression MakeVaribaleExpr(string name)
        {
            var vari = MSAst.Expression.Parameter(typeof(object), name);
            localVars.Add(vari);
            localVarsDict.Add(name, vari);

            return vari;
        }

        public void AddVariable(SBSVariable vari, bool declareInScope = true)
        {
            localVarsDict.Add(vari.Name, vari.Expr);

            if (declareInScope)
            {
                MSAst.ParameterExpression para;
                if ((para = vari.Expr as ParameterExpression) != null)
                    localVars.Add(para);
            }
        }

        public void AddVariable(IList<SBSVariable> varis)
        {
            foreach (SBSVariable vari in varis)
            {
                AddVariable(vari);
            }
        }
    }
}
