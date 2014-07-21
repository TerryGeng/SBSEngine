using System;
using MSAst = System.Linq.Expressions;
using SBSEngine.Tokenization;
using SBSEngine.Parsing.Ast;
using SBSEngine.Runtime;
using System.Diagnostics;

namespace SBSEngine.Parsing.Packer
{
    internal static class ExpressionPacker
    {

        #region BinaryExpression
        /*
         * Expression(Operator) level(High to low):
         * 6 AssignExpr
         * 3 ConditionalExpr
         * 2 Comparison
         * 1 ArithExpr
         */
        const int MAX_LEVEL = 6;

        public static MSAst.Expression Pack(ParsingContext context, Scope scope)
        {
            return PackLevel(context,scope,MAX_LEVEL);
        }

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
                op = PeekSBSOperator(context, out opLevel);

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
                                context.Error.ThrowUnexpectedTokenException(context.PeekToken(), "Unexpected operator.");
                        }
                        else
                            mainExpr = currentExpr;
                    }
                    else
                        mainExpr = MakeBinaryExpr(mainExpr, currentExpr, op, context);
                }
                else
                {
                    context.Error.ThrowUnexpectedTokenException(context.PeekToken(), "Invalid expression term.");
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
                    return ExpressionPacker.PackVariable(context, scope);
                default:
                    return PackConstantFactor(context);
            }
        }

        /*
         * ConstantFactor = (Integer|Double)
         */
        private static MSAst.Expression PackConstantFactor(ParsingContext context)
        {
            Token token = context.PeekToken();

            switch ((LexiconType)token.Type)
            {
                case LexiconType.LInteger:
                    context.NextToken();
                    return MSAst.Expression.Constant((object)Int32.Parse(token.Value));
                case LexiconType.LFloat:
                    context.NextToken();
                    return MSAst.Expression.Constant((object)Double.Parse(token.Value));
                case LexiconType.LString:
                    context.NextToken();
                    return MSAst.Expression.Constant(token.Value);
                default:
                    context.Error.ThrowUnexpectedTokenException(context.PeekToken(), "Invalid constant value factor.");
                    return null;
            }
        }

        private static MSAst.Expression PackExprFactor(ParsingContext context,Scope scope)
        {
            context.NextTokenType(LexiconType.LSLRoundBracket);
            MSAst.Expression expr = Pack(context, scope);
            context.NextTokenType(LexiconType.LSRRoundBracket, "Invalid expression factor ending.");

            return expr;
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
                            context.Error.ThrowUnexpectedTokenException(context.PeekToken(), "Invaild expression.");
                    }
                    else
                    {
                        context.Error.ThrowUnexpectedTokenException(context.PeekToken(), "Unexpected Assign operator. Only variable can be left value.");
                    }
                    return null;
            }

            return new BinaryExpression(left, right, op, context);
        }
        #endregion

        #region Operator
        private static SBSOperator PeekSBSOperator(ParsingContext context, out int level)
        {
            switch (context.PeekTokenType())
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
                case LexiconType.LSGreaterEqual:
                    level = 2;
                    return SBSOperator.GreaterThanOrEqual;
                case LexiconType.LSLessEqual:
                    level = 2;
                    return SBSOperator.LessThanOrEqual;
                case LexiconType.LSLessGreater:
                    level = 2;
                    return SBSOperator.NotEqual;
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
