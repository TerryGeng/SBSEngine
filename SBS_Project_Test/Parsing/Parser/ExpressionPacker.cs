using System;
using MSAst = System.Linq.Expressions;
using SBSEngine.Tokenization;
using SBSEngine.Parsing.Ast;
using SBSEngine.Runtime;

namespace SBSEngine.Parsing.Packer
{
    internal static class ExpressionStmtPacker
    {
        public static MSAst.Expression PackStatment(ParsingContext content)
        {
            return null; // TODO
        }

        /*
         * Variable =  '$'   Name   (['(' (String|Integer) ')'])*
         *            (*1*)  (*2*)            (*3*)<TODO>
         */
        public static MSAst.Expression PackVariable(ParsingContext context, Scope scope)
        {
            context.NextTokenType(LexiconType.LSDollar);

            string name = context.NextToken(LexiconType.LName, "Unexpected variable name.").Value;

            return new VariableAccess(name, context, scope);
        }
    }

    internal static class BinaryExprPacker
    {
        /* 
         * BinaryExpr = ['+'|'-'] Term   (('+'|'-') Term)*
         *                  (*1*)              (*2*)
         */
        public static MSAst.Expression Pack(ParsingContext context, Scope scope)
        {
            MSAst.Expression mainExpr = null;
            MSAst.Expression currentExpr = null;
            SBSOperator op;

            while (true)
            {
                op = PeekSBSOperator(context);

                if (IsTermOperator(op))
                {
                    context.NextToken();
                }
                else
                {
                    if (op != SBSOperator.Null)
                    {
                        if (IsAssignOperator(op))
                        {
                            context.NextToken();
                            if (mainExpr is VariableAccess)
                            {
                                return new AssignExpression((VariableAccess)mainExpr, Pack(context, scope), op);
                            }
                            else
                            {
                                context.Error.ThrowUnexpectedTokenException("Unexpected Assign operator. Only variable can be left value.");
                                return null;
                            }
                        }
                        else
                        {
                            context.Error.ThrowUnexpectedTokenException("Unexpected expression operator.");
                        }
                    }
                    else
                    {
                        if (mainExpr == null) // (*1*)
                            op = SBSOperator.Add;
                        else
                            return mainExpr;
                    }
                }


                if ((currentExpr = PackTerm(context, scope)) != null)
                {
                    if (mainExpr == null)
                        mainExpr = currentExpr;
                    else
                        mainExpr = new BinaryExpression(mainExpr, currentExpr, op, context);
                }
                else
                {
                    context.Error.ThrowUnexpectedTokenException("Invalid expression term.");
                }

            }
        }

        /* 
         * Term = Factor   (('*'|'/') Factor)*
         *          (*1*)      (*2*)
         */
        private static MSAst.Expression PackTerm(ParsingContext context ,Scope scope)
        {
            MSAst.Expression factors = null;
            MSAst.Expression factor = null;
            SBSOperator op;

            // Dealing with (*1*).
            if ((factors = PackFactor(context ,scope)) == null)
                return null;

            // Dealing with (*2*).
            while (true)
            {
                op = PeekSBSOperator(context);
                if (!IsFactorOperator(op))
                    return factors;
                else
                    context.NextToken();

                factor = PackFactor(context,scope);

                if (factor != null)
                {
                    factors = new BinaryExpression(factors,factor,op,context);
                }
                else
                    context.Error.ThrowUnexpectedTokenException("Invalid factor term.");
            }
        }

        private static MSAst.Expression PackFactor(ParsingContext context, Scope scope)
        {
            switch (context.PeekTokenType())
            {
                case LexiconType.LSLRoundBracket:
                    return PackExprFactor(context, scope);
                case LexiconType.LSDollar:
                    return ExpressionStmtPacker.PackVariable(context, scope);
                default:
                    return PackConstantFactor(context);
            }
        }

        /*
         * ConstantFactor = (Integer|Double)
         */
        private static MSAst.Expression PackConstantFactor(ParsingContext content)
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

        private static MSAst.Expression PackExprFactor(ParsingContext content,Scope scope)
        {
            content.NextTokenType(LexiconType.LSLRoundBracket);
            MSAst.Expression expr = Pack(content, scope);
            content.NextTokenType(LexiconType.LSRRoundBracket, "Invalid expression factor ending.");

            return expr;
        }

        #region Operator

        /* Note: Why There Isn't A 'NextSBSOperator':
         *       Consider that a low-level packer(e.g. factor packer) may get a high-level operator(e.g. '+')
         *       and in this static class we can not store any information, thus whether eat this token or not should
         *       be judged by packer itself. That's why we don't have a 'NextSBSOperator'.
         *       Besides, the only aim of using 'NextSBSOperator', which is to read a 'Two-part operator'(e.g. '+='),
         *       has been already assigned to Tokenizer. So there is no reason to use 'NextSBSOperator'.
         */
        private static SBSOperator PeekSBSOperator(ParsingContext content) 
        {
            switch (content.PeekTokenType())
            {
                case LexiconType.LSPlus:
                    return SBSOperator.Add;
                case LexiconType.LSMinus:
                    return SBSOperator.Subtract;
                case LexiconType.LSAsterisk:
                    return SBSOperator.Multiply;
                case LexiconType.LSSlash:
                    return SBSOperator.Divide;
                case LexiconType.LSEqual:
                    return SBSOperator.Assign;
                case LexiconType.LSDoubleEqual:
                    return SBSOperator.Equal;
                case LexiconType.LSPlusEqual:
                    return SBSOperator.AddAssign;
            }

            return 0;
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

        private static bool IsAssignOperator(SBSOperator op)
        {
            if (op == SBSOperator.Assign || op == SBSOperator.AddAssign)
                return true;
            return false;
        }
        #endregion

    }
}
