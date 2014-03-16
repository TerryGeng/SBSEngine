using System;
using MSAst = System.Linq.Expressions;
using SBSEngine.Tokenization;
using SBSEngine.Parsing.Ast;
using SBSEngine.Runtime;
using System.Diagnostics;

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
         * Expression level(High to low):
         * AssignExpr
         * ConditionalExpr
         * Comparison
         * ArithExpr
         */
        public static MSAst.Expression Pack(ParsingContext context, Scope scope)
        {
            return PackLevel(context,scope,6);
        }

        // /*
        // * Return an AssignExpr or a lower-level expr.
        // * 
        // * AssignExpr = Variable AssignOp ArithExpr
        // *                (*1*)   (*2*)     (*3*)
        // *                
        // * This function will return a expr from PackComparison(if don't have 1 and 2) or a AssignExpr.
        // */
        //public static MSAst.Expression PackAssign(ParsingContext context, Scope scope)
        //{
        //    MSAst.Expression first = PackComparison(context, scope);

        //    if (first == null)
        //        context.Error.ThrowUnexpectedTokenException("Invaild expression.");

        //    SBSOperator op = PeekSBSOperator(context);
        //    if (IsAssignOperator(op))
        //    {
        //        if (first is VariableAccess)
        //        {
        //            context.NextToken();

        //            MSAst.Expression second = Pack(context, scope);
        //            if (second != null)
        //                return new AssignExpression((VariableAccess)first, second, op);
        //            else
        //                context.Error.ThrowUnexpectedTokenException("Invaild expression.");
        //        }
        //        else
        //        {
        //            context.Error.ThrowUnexpectedTokenException("Unexpected Assign operator. Only variable can be left value.");
        //            return null;
        //        }
        //    }

        //    return first;
        //}

        // /*
        // * Return a Comparison or a lower-level expr from PackArith.
        // * Comparison = ArithExpr ComparisonOp ArithExpr
        // *                (*1*)      (*2*)       (*3*)
        // */
        //public static MSAst.Expression PackComparison(ParsingContext context, Scope scope)
        //{
        //    MSAst.Expression first = PackArith(context,scope);

        //    if (first == null) 
        //        context.Error.ThrowUnexpectedTokenException("Invaild expression.");

        //    SBSOperator op = PeekSBSOperator(context);
        //    if (IsComparisonOperator(op))
        //    {
        //        context.NextToken();
        //        MSAst.Expression second = PackArith(context,scope);

        //        if (first == null)
        //            context.Error.ThrowUnexpectedTokenException("Invaild expression.");

        //        return new BinaryExpression(first, second, op, context);
        //    }

        //    return first;
        //}


        // /* 
        // * ArithExpr = ['+'|'-'] Term   [('+'|'-') Term]*
        // *                  (*1*)              (*2*)
        // */
        //public static MSAst.Expression PackArith(ParsingContext context, Scope scope)
        //{
        //    MSAst.Expression mainExpr = null;
        //    MSAst.Expression currentExpr = null;
        //    SBSOperator op;

        //    while (true)
        //    {
        //        op = PeekSBSOperator(context);

        //        if (!IsTermOperator(op))
        //        {
        //            if (op == SBSOperator.Null && mainExpr == null) // (*1*)
        //                op = SBSOperator.Add;
        //            else
        //                return mainExpr;
        //        }
        //        else
        //        {
        //            context.NextToken();
        //        }


        //        if ((currentExpr = PackTerm(context, scope)) != null)
        //        {
        //            if (mainExpr == null)
        //                mainExpr = currentExpr;
        //            else
        //                mainExpr = new BinaryExpression(mainExpr, currentExpr, op, context);
        //        }
        //        else
        //        {
        //            context.Error.ThrowUnexpectedTokenException("Invalid expression term.");
        //        }

        //    }
        //}

        // /* 
        // * Term = Factor   [('*'|'/') Factor]*
        // *          (*1*)      (*2*)
        // */
        //private static MSAst.Expression PackTerm(ParsingContext context ,Scope scope)
        //{
        //    MSAst.Expression factors = null;
        //    MSAst.Expression factor = null;
        //    SBSOperator op;

        //    // Dealing with (*1*).
        //    if ((factors = PackFactor(context ,scope)) == null)
        //        return null;

        //    // Dealing with (*2*).
        //    while (true)
        //    {
        //        op = PeekSBSOperator(context);
        //        if (!IsFactorOperator(op))
        //            return factors;
        //        else
        //            context.NextToken();

        //        factor = PackFactor(context,scope);

        //        if (factor != null)
        //        {
        //            factors = new BinaryExpression(factors,factor,op,context);
        //        }
        //        else
        //            context.Error.ThrowUnexpectedTokenException("Invalid factor term.");
        //    }
        //}

        private static MSAst.Expression PackLevel(ParsingContext context, Scope scope, int level)
        {
            if (level == -1)
                return PackFactor(context, scope);

            MSAst.Expression mainExpr = null;
            MSAst.Expression currentExpr = null;
            SBSOperator op;
            int opLevel;

            while (true)
            {
                op = PeekSBSOperatorWithLevel(context, out opLevel);

                if (opLevel == -1)
                {
                    if (mainExpr != null)
                        return mainExpr;
                }
                else if (opLevel > level) return mainExpr;
                else if (opLevel == level) context.NextToken();
                else if (opLevel < level) op = SBSOperator.Null; // This operator belongs to lower-level packer. Ignore it.


                if ((currentExpr = PackLevel(context, scope, level - 1)) != null)
                {
                    if (mainExpr == null)
                    {
                        if (op != SBSOperator.Null)
                        {
                            if (level == 1 && op == SBSOperator.Subtract)
                                mainExpr = new BinaryExpression(MSAst.Expression.Constant(0), currentExpr, SBSOperator.Subtract, context);
                            else
                                context.Error.ThrowUnexpectedTokenException("Unexpected operator.");
                        }
                        else
                            mainExpr = currentExpr;
                    }
                    else
                        mainExpr = MakeBinaryExpr(mainExpr, currentExpr, op, context);
                }
                else
                {
                    context.Error.ThrowUnexpectedTokenException("Invalid expression term.");
                }

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

        private static MSAst.Expression MakeBinaryExpr(MSAst.Expression left, MSAst.Expression right, SBSOperator op, ParsingContext context)
        {
            switch (op)
            {
                case SBSOperator.Assign:
                case SBSOperator.AddAssign:
                    if (left is VariableAccess)
                    {
                        if (right != null)
                            return new AssignExpression((VariableAccess)left, right, op);
                        else
                            context.Error.ThrowUnexpectedTokenException("Invaild expression.");
                    }
                    else
                    {
                        context.Error.ThrowUnexpectedTokenException("Unexpected Assign operator. Only variable can be left value.");
                    }
                    return null;
            }

            return new BinaryExpression(left, right, op, context);
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
                case LexiconType.LSGreater:
                    return SBSOperator.GreaterThan;
            }

            return 0;
        }

        private static SBSOperator PeekSBSOperatorWithLevel(ParsingContext content, out int level)
        {
            switch (content.PeekTokenType())
            {
                case LexiconType.LSPlus:
                    level = 1;
                    return SBSOperator.Add;
                case LexiconType.LSMinus:
                    level = 1;
                    return SBSOperator.Subtract;
                case LexiconType.LSAsterisk:
                    level = 0;
                    return SBSOperator.Multiply;
                case LexiconType.LSSlash:
                    level = 0;
                    return SBSOperator.Divide;
                case LexiconType.LSEqual:
                    level = 6;
                    return SBSOperator.Assign;
                case LexiconType.LSDoubleEqual:
                    level = 3;
                    return SBSOperator.Equal;
                case LexiconType.LSPlusEqual:
                    level = 6;
                    return SBSOperator.AddAssign;
                case LexiconType.LSGreater:
                    level = 2;
                    return SBSOperator.GreaterThan;
                case LexiconType.LSLess:
                    level = 2;
                    return SBSOperator.LessThan;
            }

            level = -1;
            return 0;
        }

        public static SBSOperator GetFirstOp(int level)
        {
            switch (level)
            {
                case 1: return SBSOperator.Add;
            }

            return SBSOperator.Null;
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

        private static bool IsComparisonOperator(SBSOperator op)
        {
            if (op == SBSOperator.Equal || op == SBSOperator.NotEqual || op == SBSOperator.GreaterThan ||
                op == SBSOperator.GreaterThanOrEqual || op == SBSOperator.LessThan || op == SBSOperator.LessThanOrEqual)
                return true;
            return false;
        }

        #endregion

    }
}
