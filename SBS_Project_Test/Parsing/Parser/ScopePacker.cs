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
        public static MSAst.Expression Pack(ParsingContext context)
        {
            return Pack(context, new Scope());
        }

        public static MSAst.Expression Pack(ParsingContext context, Scope scope)
        {

            var list = new LinkedList<MSAst.Expression>();
            TokenDetail token = context.PeekTokenDetail();

            while (true)
            {
                switch (token.AbstractType)
                {
                    default:
                        list.AddLast(BinaryExprPacker.Pack(context, scope));
                        break;
                }

                if (!context.MaybeNext(LexiconType.LLineBreak))
                    if (context.NextTokenType(LexiconType.Null, "Unexpected statment end."))
                        break;
            }

            return new ScopeStatment(list, scope);
        }
    }
}
