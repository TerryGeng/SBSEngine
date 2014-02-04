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
        public static MSAst.Expression PackScope(ParsingContext context)
        {
            var list = new LinkedList<MSAst.Expression>();
            TokenDetail token = context.PeekTokenDetail();

            while (true)
            {
                switch (token.AbstractType)
                {
                    case AbstractTokenType.Number:
                        list.AddLast(BinaryExprPacker.PackBinaryExpr(context));
                        break;
                }

                if (!context.MaybeNext(LexiconType.LLineBreak))
                    if (context.NextTokenType(LexiconType.Null, "Unexpected statment end."))
                        break;
            }

            return new ScopeStatment(list);
        }
    }
}
