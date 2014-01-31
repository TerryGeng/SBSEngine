using System;
using System.Collections.Generic;
using MSAst = System.Linq.Expressions;
using SBSEngine.Tokenization;
using SBSEngine.Parsing.Ast;
using SBSEngine.Runtime;

namespace SBSEngine.Parsing.Parser
{
    internal static class ScopePacker
    {
        public MSAst.Expression PackScope(ParsingContext context)
        {
            var list = new LinkedList<MSAst.Expression>();
            AdvToken token;

            while ((token = context.NextAdvToken()).Type != LexiconType.Null)
            {
                switch (token.AbstractType)
                {
                    case AbstractTokenType.Number:
                        list.AddLast(BinaryExprPacker.PackBinaryExpr(context));
                        break;
                }
            }

            return new ScopeStatment(list);
        }
    }
}
