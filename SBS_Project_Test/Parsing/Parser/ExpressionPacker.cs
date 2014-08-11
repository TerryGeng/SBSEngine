using System;
using MSAst = System.Linq.Expressions;
using SBSEnvironment.Tokenization;
using SBSEnvironment.Parsing.Ast;
using SBSEnvironment.Runtime;
using System.Diagnostics;
using System.Collections.Generic;

namespace SBSEnvironment.Parsing
{
    partial class Parser
    {
        #region BinaryExpression
        /*
         * Expression(Operator) level(High to low):
         * 6 AssignExpr
         * 3 ConditionalExpr
         * 2 Comparison
         * 1 ArithExpr
         */
        const int EXPR_MAX_LEVEL = 6;

        private MSAst.Expression PackExpr(Scope scope)
        {
            return PackExprLevel(scope,EXPR_MAX_LEVEL);
        }

        private MSAst.Expression PackExprLevel(Scope scope, int level)
        {
            if (level == -1)
                return PackFactor(scope);

            MSAst.Expression mainExpr = null;
            MSAst.Expression currentExpr = null;
            SBSOperator op;
            int opLevel;

            while (true)
            {
                op = PeekSBSOperator(out opLevel);

                if (opLevel == -1)
                {
                    if (mainExpr != null)
                        return mainExpr;
                }
                else if (opLevel > level) return mainExpr;
                else if (opLevel == level) context.NextToken();
                else if (opLevel < level) op = SBSOperator.Null; // This operator belongs to lower-level packer. Ignore it.


                if ((currentExpr = PackExprLevel(scope, level - 1)) != null)
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

        private MSAst.Expression MakeBinaryExpr(MSAst.Expression left, MSAst.Expression right, SBSOperator op, ParsingContext context)
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

        #region Factor

        private MSAst.Expression PackFactor(Scope scope)
        {
            switch (context.PeekTokenType())
            {
                case LexiconType.LSLRoundBracket:
                    return PackExprFactor(scope);
                case LexiconType.LSDollar:
                    return PackVariableAccess(scope);
                case LexiconType.LName:
                    return PackFunctionInvoke(scope);
                default:
                    return PackConstantFactor();
            }
        }

        /*
         * ConstantFactor = (Integer|Double)
         */
        private MSAst.Expression PackConstantFactor()
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

        private  MSAst.Expression PackExprFactor(Scope scope)
        {
            context.NextTokenType(LexiconType.LSLRoundBracket);
            MSAst.Expression expr = PackExpr(scope);
            context.NextTokenType(LexiconType.LSRRoundBracket, "Invalid expression factor ending.");

            return expr;
        }

        /*
        * Variable =  '$'   Name   (['(' (String|Integer) ')'])*
        *            (*1*)  (*2*)            (*3*)<TODO>
        */
        private MSAst.Expression PackVariableAccess(Scope scope)
        {
            return new VariableAccess(PackVariable(), context, scope);
        }

        private string PackVariable()
        {
            if (context.MaybeNext(LexiconType.LSDollar))
                return context.NextToken(LexiconType.LName, "Unexpected variable name.").Value;
            else
                return null;
        }

        private MSAst.Expression PackFunctionInvoke(Scope scope)
        {
            string name;
            List<MSAst.Expression> argsList;

            name = context.NextToken(LexiconType.LName).Value;
            argsList = PackArgsInvokeList(scope);

            return new FunctionInvoke(name, argsList, context);
        }

        private List<MSAst.Expression> PackArgsInvokeList(Scope scope)
        {
            List<MSAst.Expression> argsArray = null;
            MSAst.Expression arg;

            context.NextToken(LexiconType.LSLRoundBracket);

            if ((arg = PackExpr(scope)) != null)
            {
                argsArray = new List<MSAst.Expression>();
                argsArray.Add(arg);

                while (context.MaybeNext(LexiconType.LSComma))
                {
                    arg = PackExpr(scope);
                    argsArray.Add(arg);
                }
            }

            context.NextToken(LexiconType.LSRRoundBracket);

            return argsArray;
        }



        #endregion

        #region Operator
        private SBSOperator PeekSBSOperator(out int level)
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

        private bool IsTermOperator(SBSOperator op)
        {
            if (op == SBSOperator.Add || op == SBSOperator.Subtract)
                return true;
            return false;
        }

        private bool IsFactorOperator(SBSOperator op)
        {
            if (op == SBSOperator.Multiply || op == SBSOperator.Divide)
                return true;
            return false;
        }

        private bool IsAssignOperator(SBSOperator op)
        {
            if (op == SBSOperator.Assign || op == SBSOperator.AddAssign)
                return true;
            return false;
        }

        private bool IsComparisonOperator(SBSOperator op)
        {
            if (op == SBSOperator.Equal || op == SBSOperator.NotEqual || op == SBSOperator.GreaterThan ||
                op == SBSOperator.GreaterThanOrEqual || op == SBSOperator.LessThan || op == SBSOperator.LessThanOrEqual)
                return true;
            return false;
        }
        #endregion
    }

}
