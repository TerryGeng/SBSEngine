﻿using System;
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
        private bool ifInFunc;

        private MSAst.Expression PackScope(Scope parent = null, bool createChildScope = true)
        {
            Scope scope;

            if (createChildScope) scope = new Scope(parent);
            else scope = parent;

            var list = new LinkedList<MSAst.Expression>();
            Token token;

            while (true)
            {
                token = context.PeekToken();
                switch ((LexiconType)token.Type)
                {
                    case LexiconType.LKIf:
                        list.AddLast(PackIf(scope));
                        break;
                    case LexiconType.LKWhile:
                        list.AddLast(PackWhile(scope));
                        break;
                    case LexiconType.LKBreak:
                        list.AddLast(PackBreak(scope));
                        break;
                    case LexiconType.LKContinue:
                        list.AddLast(PackContinue(scope));
                        break;
                    case LexiconType.LKFunction:
                        if (!ifInFunc)
                        {
                            ifInFunc = true;
                            PackFunction();
                            ifInFunc = false;
                        }
                        else
                        {
                            context.Error.ThrowUnexpectedTokenException(token, "Unexpected function declaration.");
                        }
                        break;
                    case LexiconType.LKReturn:
                        if (!ifInFunc) context.Error.ThrowUnexpectedTokenException(token, "Unexpected 'return'.");
                        list.AddLast(PackReturn(scope));
                        break;
                    case LexiconType.LKEnd:
                    case LexiconType.LKElse:
                        return new ScopeStatment(list, scope);
                    default:
                        list.AddLast(PackExpr(scope));
                        break;
                }

                if (!context.MaybeNext(LexiconType.LLineBreak))
                    if (context.NextTokenType(LexiconType.Null, "Unexpected statment end."))
                        break;
            }

            return new ScopeStatment(list, scope);
        }

        /*
         * IfStmt = 'If' + Expression + 'Then' + LineBreak +
         *               Statments +                                     --(1)--
         *           [('Else' + 'If' + Expression + 'Then'  + LineBreak
         *               Statments)*]                                    --(2)--
         *           ['Else' + LineBreak +
         *               Statments]                                      --(3)--
         *          'End' + 'If' + LineBreak                             --(4)--
         * 
         */
        private MSAst.Expression PackIf(Scope scope)
        {
            MSAst.Expression condition = null;
            MSAst.Expression then = null;
            MSAst.Expression elseStmt = null;
            // --(1)--
            context.NextToken(LexiconType.LKIf);

            condition = PackExpr(scope);
            context.NextToken(LexiconType.LKThen, "Expected 'Then'.");
            context.NextToken(LexiconType.LLineBreak);

            then = PackScope(scope);

            if (!context.MaybeNext(LexiconType.LKEnd))
            {
                if (context.MaybeNext(LexiconType.LKElse))
                {
                    if (context.PeekToken(LexiconType.LKIf))
                    {
                        //--(2)--
                        elseStmt = PackIf(scope);
                        return new IfStatment(condition, then, elseStmt);
                    }
                    else if (context.MaybeNext(LexiconType.LLineBreak))
                    {
                        //--(3)--
                        elseStmt = PackScope(scope);
                        context.NextToken(LexiconType.LKEnd);
                        context.NextToken(LexiconType.LKIf, "Unexpected 'End' instruction for 'If' statment.");
                        return new IfStatment(condition, then, elseStmt);
                    }

                    context.Error.ThrowUnexpectedTokenException(context.PeekToken(), "Expected Statments for 'Else'.");
                }
            }
            else
            {
                context.NextToken(LexiconType.LKIf, "Unexpected 'End' instruction for 'If' statment.");
                return new IfStatment(condition, then);
            }

            Debug.Assert(false);
            return null;
        }

        /*
         * WhileStmt = 'While' + Expression(condition) + \n + 
         *                  Statments(body) + 
         *             'End' + 'While' + \n
         */
        private MSAst.Expression PackWhile(Scope scope)
        {
            MSAst.Expression condition;
            MSAst.Expression body;

            var breakLabel = MSAst.Expression.Label();
            var continueLabel = MSAst.Expression.Label();

            Scope childScope = new Scope(scope);

            childScope.BreakLabel = breakLabel;
            childScope.ContinueLabel = continueLabel;

            context.NextToken(LexiconType.LKWhile);

            condition = PackExpr(scope);
            context.NextToken(LexiconType.LLineBreak);

            body = PackScope(childScope,false);

            context.NextToken(LexiconType.LKEnd);
            context.NextToken(LexiconType.LKWhile, "Unexpected 'End' instruction for 'While' statment.");

            return new WhileStatment(condition, body, breakLabel, continueLabel);
        }

        private MSAst.Expression PackBreak(Scope scope)
        {
            var token = context.NextToken(LexiconType.LKBreak);

            var label = scope.BreakLabel;

            if (label == null) context.Error.ThrowUnexpectedTokenException(token, "Not in a loop.");

            return MSAst.Expression.Break(label);
        }

        private MSAst.Expression PackContinue(Scope scope)
        {
            var token = context.NextToken(LexiconType.LKContinue);

            var label = scope.ContinueLabel;

            if (label == null) context.Error.ThrowUnexpectedTokenException(token, "Not in a loop.");

            return MSAst.Expression.Continue(label);
        }

        private MSAst.Expression PackReturn(Scope scope)
        {
            var token = context.NextToken(LexiconType.LKReturn);
            var label = scope.ReturnLabel;

            if (context.PeekToken(LexiconType.LLineBreak))
            {
                return MSAst.Expression.Return(label);
            }
            else
            {
                var val = PackExpr(scope);
                return MSAst.Expression.Return(label, MSAst.Expression.Convert(val, typeof(object)));
            }
        }
    }
}
