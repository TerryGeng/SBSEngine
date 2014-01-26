using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SBSEngine.Tokenization;
using SBSEngine.Runtime;

namespace SBSEngine.Parsing
{
    public class UnexpectedTokenException : ApplicationException
    {
        public Token Token { get; private set; }
        public int LineNumber { get; private set; }
        public int Position { get; private set; }

        public static Tokenizer Tokenizer { private get; set; }


        public UnexpectedTokenException(int position, int lineNum, Token token, string message = null)
            : base(message)
        {
            this.Token = token;
            this.LineNumber = lineNum;
            this.Position = position;
        }
    }

    class ThrowHelper
    {
        public static Tokenizer Tokenizer { private get; set; }
        public static int LineNum { get; set; }

        public static void ThrowUnexpectedTokenException(string message = null)
        {
            ThrowUnexpectedTokenException(Tokenizer.PeekToken(), message);
        }

        public static void ThrowUnexpectedTokenException(Token token, string message = null)
        {
            throw new UnexpectedTokenException(Tokenizer.Position, LineNum, token,
                String.Format("Parsing Exception: Unexpected '{0}' on line {1}, position {2}. {3}",
                    ((LexiconType)token.Type).ToString(), LineNum, Tokenizer.Position, message)
            );
        }
    }
    //class StatmentPacker : IPacker
    //{
    //    private static StatmentPacker instance;

    //    public Tokenizer Tokenizer { get; private set; }

    //    public IPacker Instance  // TODO: Use Cache<T> to replace.
    //    {
    //        get
    //        {
    //            if (instance == null)
    //                instance = new StatmentPacker();
    //            return instance;
    //        }
    //    }

    //}

    internal class ExpressionPacker : IPacker
    {
        public Tokenizer Tokenizer { set; private get; }

        public IStatment PackStatment()
        {
            return null;
        }

        /* 
         * Expression = ['+'|'-'] Term   (('+'|'-') Term)*
         *                  (*1*)                 (*2*)
         */
        internal Expression PackExpression()
        {
            ThrowHelper.Tokenizer = Tokenizer; // TODO: This line is for debug. Remove this.

            Expression mainExpr = null;
            Expression currentExpr = null;
            LexiconType opType;

            while (true)
            {
                opType = (LexiconType)Tokenizer.PeekTokenType();

                if (!IsTermOperator(opType))
                    if (mainExpr == null) // (*1*)
                        opType = LexiconType.LSPlus;
                    else
                        return mainExpr;
                else
                    Tokenizer.NextToken();

                if ((currentExpr = PackTerm()) != null)
                {
                    mainExpr = MakeBinaryExpression(opType, mainExpr, currentExpr);
                }
                else
                {
                    ThrowHelper.ThrowUnexpectedTokenException("Invalid expression term.");
                }

            }
        }

        /* 
         * Term = Factor   (('*'|'/') Factor)*
         *          (*1*)      (*2*)
         */
        private Expression PackTerm()
        {
            Expression factors = null;
            Expression factor = null;
            LexiconType opType;

            // Dealing with (*1*).
            if ((factors = PackFactor()) == null)
                return null;

            // Dealing with (*2*).
            while (true)
            {
                opType = (LexiconType)Tokenizer.PeekTokenType();
                if (!IsFactorOperator(opType))
                    return factors;
                else
                    Tokenizer.NextToken();

                factor = PackFactor();

                if (factor != null)
                {
                    factors = MakeBinaryExpression(opType,factors,factor);
                }
                else
                    ThrowHelper.ThrowUnexpectedTokenException("Invalid factor term.");
            }
        }

        private Expression PackFactor()
        {
            switch ((LexiconType)Tokenizer.PeekTokenType())
            {
                case LexiconType.LSLRoundBracket:
                    return PackExprFactor();
                default:
                    return PackConstantFactor();
            }
        }

        /*
         * ConstantFactor = (Integer|Double)
         */
        private Expression PackConstantFactor()
        {
            Token token = Tokenizer.PeekToken();

            switch ((LexiconType)token.Type)
            {
                case LexiconType.LInteger:
                    Tokenizer.NextToken();
                    return Expression.Constant(Int32.Parse(token.Value));
                case LexiconType.LFloat:
                    Tokenizer.NextToken();
                    return Expression.Constant(Double.Parse(token.Value));
                case LexiconType.LString:
                    Tokenizer.NextToken();
                    return Expression.Constant(token.Value);
                default:
                    ThrowHelper.ThrowUnexpectedTokenException("Invalid constant value factor.");
                    return null;
            }
        }

        private Expression PackExprFactor()
        {
            Tokenizer.NextToken();

            Expression expr = PackExpression();

            if (Tokenizer.NextTokenType() != (int)LexiconType.LSRRoundBracket)
                ThrowHelper.ThrowUnexpectedTokenException("Invalid expression factor ending.");

            return expr;
        }

        // ------ Pack Binary Expression ------
        // TODO: Maybe should move this out of this class.

        private Expression MakeBinaryExpression(LexiconType op, Expression left, Expression right)
        {
            switch (op)
            {
                case LexiconType.LSPlus:
                    return OperationAdd(left,right);
                case LexiconType.LSMinus:
                    return OperationSub(left, right);
                case LexiconType.LSAsterisk:
                    return OperationMultiply(left, right);
                case LexiconType.LSSlash:
                    return OperationDivide(left, right);

            }

            return null;
        }

        private Expression OperationAdd(Expression left,Expression right)
        {
            if (left == null)
            {
                return right;
            }

            if (TypeConversion.Implicit(ref left, ref right) == TypeConversion.AbstractType.Numeric)
            {
                return Expression.Add(left, right);
            }
            else
            {
                throw new ApplicationException("Implicit conversion error.");
            }
        }

        private Expression OperationSub(Expression left, Expression right)
        {
            if (left == null)
            {
                left = Expression.Constant(0);
            }

            if (TypeConversion.Implicit(ref left, ref right) == TypeConversion.AbstractType.Numeric)
            {
                return Expression.Subtract(left, right);
            }
            else
            {
                throw new ApplicationException("Implicit conversion error.");
            }
        }

        private Expression OperationMultiply(Expression left, Expression right)
        {
            if (TypeConversion.Implicit(ref left, ref right) == TypeConversion.AbstractType.Numeric)
            {
                return Expression.Multiply(left, right);
            }
            else
            {
                throw new ApplicationException("Implicit conversion error.");
            }
        }

        private Expression OperationDivide(Expression left, Expression right)
        {
            if (TypeConversion.Implicit(ref left, ref right) == TypeConversion.AbstractType.Numeric)
            {
                return Expression.Divide(left, right);
            }
            else
            {
                throw new ApplicationException("Implicit conversion error.");
            }
        }

        private bool IsFactorOperator(LexiconType op)
        {
            switch (op)
            {
                case LexiconType.LSAsterisk:
                case LexiconType.LSSlash:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsTermOperator(LexiconType op)
        {
            switch (op)
            {
                case LexiconType.LSPlus:
                case LexiconType.LSMinus:
                    return true;
                default:
                    return false;
            }
        }

    }
}
