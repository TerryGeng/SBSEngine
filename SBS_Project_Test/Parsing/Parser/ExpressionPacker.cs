using System;
using System.Collections.Generic;
using System.Linq;
using MSAst = System.Linq.Expressions;
using System.Text;
using SBSEngine.Tokenization;
using SBSEngine.Parsing.Ast;

namespace SBSEngine.Parsing
{
    internal class ExpressionStmtPacker : Packer
    {

    }

    internal class BinaryExprPacker : Packer
    {
        /* 
         * BinaryExpr = ['+'|'-'] Term   (('+'|'-') Term)*
         *                  (*1*)              (*2*)
         */
        public static override MSAst.Expression PackStatment(SourceContent content)
        {
            MSAst.Expression mainExpr = null;
            MSAst.Expression currentExpr = null;
            SBSOperator op;

            while (true)
            {
                op = PeekAsSBSOperator(content);

                if (!IsTermOperator(op))
                    if (mainExpr == null) // (*1*)
                        op = SBSOperator.Add;
                    else
                        return mainExpr;
                else
                    content.NextToken();

                if ((currentExpr = PackTerm(content)) != null)
                {
                    mainExpr = new BinaryExpression(mainExpr, currentExpr, op);
                }
                else
                {
                    content.Error.ThrowUnexpectedTokenException("Invalid expression term.");
                }

            }
        }

        /* 
         * Term = Factor   (('*'|'/') Factor)*
         *          (*1*)      (*2*)
         */
        private static MSAst.Expression PackTerm(SourceContent content)
        {
            MSAst.Expression factors = null;
            MSAst.Expression factor = null;
            SBSOperator op;

            // Dealing with (*1*).
            if ((factors = PackFactor(content)) == null)
                return null;

            // Dealing with (*2*).
            while (true)
            {
                op = PeekAsSBSOperator(content);
                if (!IsFactorOperator(op))
                    return factors;
                else
                    content.NextToken();

                factor = PackFactor(content);

                if (factor != null)
                {
                    factors = new BinaryExpression(factors,factor,op);
                }
                else
                    content.Error.ThrowUnexpectedTokenException("Invalid factor term.");
            }
        }

        private static MSAst.Expression PackFactor(SourceContent content)
        {
            switch (content.PeekTokenType())
            {
                case LexiconType.LSLRoundBracket:
                    return PackExprFactor(content);
                default:
                    return PackConstantFactor(content);
            }
        }

        /*
         * ConstantFactor = (Integer|Double)
         */
        private static MSAst.Expression PackConstantFactor(SourceContent content)
        {
            Token token = content.PeekToken();

            switch ((LexiconType)token.Type)
            {
                case LexiconType.LInteger:
                    content.NextToken();
                    return MSAst.Expression.Constant((object)Int32.Parse(token.Value));
                case LexiconType.LFloat:
                    content.NextToken();
                    return MSAst.Expression.Constant((object)Double.Parse(token.Value));
                case LexiconType.LString:
                    content.NextToken();
                    return MSAst.Expression.Constant(token.Value);
                default:
                    content.Error.ThrowUnexpectedTokenException("Invalid constant value factor.");
                    return null;
            }
        }

        private static MSAst.Expression PackExprFactor(SourceContent content)
        {
            content.NextTokenType(LexiconType.LSLRoundBracket);
            MSAst.Expression expr = PackStatment(content);
            content.NextTokenType(LexiconType.LSRRoundBracket, "Invalid expression factor ending.");

            return expr;
        }

        private static SBSOperator PeekAsSBSOperator(SourceContent content)
        {
            return GetSBSOperator(content.PeekTokenType());
        }

        private static bool IsTermOperator(SBSOperator op)
        {
            if (op == SBSOperator.Add || op == SBSOperator.Subtract)
                return true;
            return false;
        }

        private static bool IsFactorOperator(SBSOperator op)
        {
            if (op == SBSOperator.Multiply || op == SBSOperator.Divide)
                return true;
            return false;
        }

        private static SBSOperator GetSBSOperator(LexiconType tokentype) // TODO: Add more.
        {
            switch (tokentype)
            {
                case LexiconType .LSPlus:
                    return SBSOperator.Add;
                case LexiconType.LSMinus:
                    return SBSOperator.Subtract;
                case LexiconType.LSAsterisk:
                    return SBSOperator.Multiply;
                case LexiconType.LSSlash:
                    return SBSOperator.Divide;
            }

            return 0;
        }

    }
}
