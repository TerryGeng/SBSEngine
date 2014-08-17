using System;
using System.Collections.Generic;
using MSAst = System.Linq.Expressions;
using SBSEnvironment.Tokenization;
using SBSEnvironment.Parsing.Ast;
using SBSEnvironment.Runtime;
using System.Diagnostics;

namespace SBSEnvironment.Parsing
{
    partial class Parser
    {
        /*
         * FunctionDeclaration = 
         *      'Function' +　Name(name) + '(' + [ Variable + [ ',' + Variable ]* ] + ')' + LineBreak +
         *          Statments
         *      'End' + 'Function' + LineBreak
         */
        private void PackFunction()
        {
            string name;
            MSAst.ParameterExpression argsArray;
            List<SBSVariable> args;
            ScopeStatment body;
            Scope scope = new Scope();

            var returnLabel = MSAst.Expression.Label(typeof(object));
            var returnVal = MSAst.Expression.Parameter(typeof(object), "@{returnVal}");

            scope.ReturnValue = returnVal;
            scope.ReturnLabel = returnLabel;

            context.NextToken(LexiconType.LKFunction);
            name = context.NextToken(LexiconType.LName, "Unexpected function name.").Value;
            args = PackArgsDecList(out argsArray);

            context.NextToken(LexiconType.LLineBreak);

            scope.AddVariable(new SBSVariable("@{args}", argsArray), false);
            scope.AddVariable(args);

            body = PackScope(scope, false) as ScopeStatment;

            context.NextToken(LexiconType.LKEnd);
            context.NextToken(LexiconType.LKFunction);

            body.AddStatment(MSAst.Expression.Label(returnLabel, MSAst.Expression.Constant(null)));

            context.ExecutableUnit.AddFunction(new Function(name, args, body));
        }

        private List<SBSVariable> PackArgsDecList(out MSAst.ParameterExpression argsArray)
        {
            argsArray = MSAst.Expression.Parameter(typeof(object[]), "@{args}");

            List<SBSVariable> nameList = new List<SBSVariable>();
            string name;
            int argsCount;

            context.NextToken(LexiconType.LSLRoundBracket);

            if ((name = PackVariable()) != null)
            {
                argsCount = 1;

                nameList = new List<SBSVariable>();
                nameList.Add(new SBSVariable(name, MakeArrayAccess(argsArray, argsCount - 1)));

                while (context.MaybeNext(LexiconType.LSComma))
                {
                    ++argsCount;
                    name = PackVariable();
                    nameList.Add(new SBSVariable(name, MakeArrayAccess(argsArray, argsCount - 1)));
                }
            }

            context.NextToken(LexiconType.LSRRoundBracket);

            return nameList;
        }

        private MSAst.IndexExpression MakeArrayAccess(MSAst.ParameterExpression array, int index)
        {
            return MSAst.Expression.ArrayAccess(array, MSAst.Expression.Constant(index));
        }
    }
}
