using System;
using System.Collections.Generic;
using MSAst = System.Linq.Expressions;
using SBSEngine.Tokenization;
using SBSEngine.Parsing.Ast;
using SBSEngine.Runtime;
using System.Diagnostics;

namespace SBSEngine.Parsing.Packer
{
    internal static class ScopePacker
    {
        public static MSAst.Expression Pack(ParsingContext context, Scope parent = null, bool createChildScope = true)
        {
            Scope scope;

            if (createChildScope) scope = new Scope(parent);
            else scope = parent;

            var list = new LinkedList<MSAst.Expression>();
            LexiconType type;

            while (true)
            {
                type = context.PeekTokenType();
                switch (type)
                {
                    case LexiconType.LKIf:
                        list.AddLast(PackIf(context, scope));
                        break;
                    case LexiconType.LKWhile:
                        list.AddLast(PackWhile(context, scope));
                        break;
                    case LexiconType.LKBreak:
                        list.AddLast(PackBreak(context, scope));
                        break;
                    case LexiconType.LKContinue:
                        list.AddLast(PackContinue(context, scope));
                        break;
                    case LexiconType.LKEnd:
                    case LexiconType.LKElse:
                        return new ScopeStatment(list, scope);
                    default:
                        list.AddLast(ExpressionPacker.Pack(context, scope));
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
        public static MSAst.Expression PackIf(ParsingContext context, Scope scope)
        {
            MSAst.Expression condition = null;
            MSAst.Expression then = null;
            MSAst.Expression elseStmt = null;
            // --(1)--
            context.NextToken(LexiconType.LKIf);

            condition = ExpressionPacker.Pack(context, scope);
            context.NextToken(LexiconType.LKThen, "Expected 'Then'.");
            context.NextToken(LexiconType.LLineBreak);

            then = Pack(context, scope);

            if (!context.MaybeNext(LexiconType.LKEnd))
            {
                if (context.MaybeNext(LexiconType.LKElse))
                {
                    if (context.PeekToken(LexiconType.LKIf))
                    {
                        //--(2)--
                        elseStmt = PackIf(context, scope).Reduce();
                        return new IfStatment(condition, then, elseStmt);
                    }
                    else if (context.MaybeNext(LexiconType.LLineBreak))
                    {
                        //--(3)--
                        elseStmt = Pack(context, scope).Reduce();
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
        public static MSAst.Expression PackWhile(ParsingContext context, Scope scope)
        {
            MSAst.Expression condition;
            MSAst.Expression body;

            var breakLabel = MSAst.Expression.Label();
            var continueLabel = MSAst.Expression.Label();

            Scope childScope = new Scope(scope);

            childScope.BreakLabel = breakLabel;
            childScope.ContinueLabel = continueLabel;

            context.NextToken(LexiconType.LKWhile);

            condition = ExpressionPacker.Pack(context,scope);
            context.NextToken(LexiconType.LLineBreak);

            body = Pack(context,childScope,false);

            context.NextToken(LexiconType.LKEnd);
            context.NextToken(LexiconType.LKWhile, "Unexpected 'End' instruction for 'While' statment.");

            return new WhileStatment(condition, body, breakLabel, continueLabel);
        }

        public static MSAst.Expression PackBreak(ParsingContext context, Scope scope)
        {
            var token = context.NextToken(LexiconType.LKBreak);

            var label = scope.BreakLabel;

            if (label == null) context.Error.ThrowUnexpectedTokenException(token, "Not in a loop.");

            return MSAst.Expression.Break(label);
        }

        public static MSAst.Expression PackContinue(ParsingContext context, Scope scope)
        {
            var token = context.NextToken(LexiconType.LKContinue);

            var label = scope.ContinueLabel;

            if (label == null) context.Error.ThrowUnexpectedTokenException(token, "Not in a loop.");

            return MSAst.Expression.Continue(label);
        }
    }
}
