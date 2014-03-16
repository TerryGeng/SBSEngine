using System;
using System.Collections.Generic;
using MSAst = System.Linq.Expressions;
using SBSEngine.Tokenization;
using SBSEngine.Parsing.Ast;
using SBSEngine.Runtime;

namespace SBSEngine.Parsing.Packer
{
    internal static class ScopePacker
    {
        public static MSAst.Expression Pack(ParsingContext context, Scope parent = null)
        {
            Scope scope = new Scope(parent);

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
                    case LexiconType.LKEnd:
                        context.NextToken();
                        return new ScopeStatment(list, scope);
                    default:
                        list.AddLast(BinaryExprPacker.Pack(context, scope).Reduce());
                        break;
                }

                if (!context.MaybeNext(LexiconType.LLineBreak))
                    if (context.NextTokenType(LexiconType.Null, "Unexpected statment end."))
                        break;
            }

            return new ScopeStatment(list, scope);
        }

        public static MSAst.Expression PackIf(ParsingContext context, Scope scope)
        {
            context.NextToken(LexiconType.LKIf);

            //var condition = new BinaryExpression(BinaryExprPacker.Pack(context, scope).Reduce(), MSAst.Expression.Constant(0), SBSOperator.NotEqual, context).Reduce();
            var condition = BinaryExprPacker.Pack(context, scope).Reduce();
            context.NextToken(LexiconType.LKThen, "Expected 'Then'.");
            context.NextToken(LexiconType.LLineBreak);
            var then = Pack(context,scope).Reduce();
            context.NextToken(LexiconType.LKIf, "Unexpected 'End' instruction for 'If' statment.");

            return MSAst.Expression.IfThen(MSAst.Expression.Convert(condition,typeof(bool)), then);
        }
    }
}
