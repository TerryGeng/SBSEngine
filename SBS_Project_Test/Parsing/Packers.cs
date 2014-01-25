using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SBSEngine.Tokenization;

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

    public class ExpressionPacker : IPacker // TODO: public(for debug) => private
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
        public Expression PackExpression() // TODO: public(for debug) => private
        {
            ThrowHelper.Tokenizer = Tokenizer; // TODO: This line is for debug. Remove this.

            Expression mainExpr = null;
            Expression currentExpr = null;
            ExpressionType type;

            // Dealing with (*1*) of the expression.
            switch ((LexiconType)Tokenizer.PeekTokenType())
            {
                case LexiconType.LSMinus:
                    Tokenizer.NextToken();
                    if ((currentExpr = PackTerm()) != null)
                        mainExpr = Expression.MakeBinary(ExpressionType.Subtract, Expression.Constant(0), currentExpr);
                    else
                        ThrowHelper.ThrowUnexpectedTokenException("Invalid expression term.");
                    break;
                case LexiconType.LSPlus:
                    Tokenizer.NextToken();
                    if ((mainExpr = PackTerm()) == null)
                        ThrowHelper.ThrowUnexpectedTokenException("Invalid expression term.");
                    break;
                default:
                    if ((mainExpr = PackTerm()) == null)
                        ThrowHelper.ThrowUnexpectedTokenException("Invalid expression term.");
                    break;
            }

            // Dealing with (*2*).

            while (true)
            {
                switch ((LexiconType)Tokenizer.PeekTokenType())
                {
                    case LexiconType.LSPlus:
                        type = ExpressionType.Add;
                        Tokenizer.NextToken();
                        break;
                    case LexiconType.LSMinus:
                        type = ExpressionType.Subtract;
                        Tokenizer.NextToken();
                        break;
                    default:
                        mainExpr = Expression.Convert(mainExpr,typeof(double)); // TODO: This line is for debug output. Remove this.
                        return mainExpr;
                }

                if ((currentExpr = PackTerm()) != null)
                {
                    ImplicitComversion(ref mainExpr, ref currentExpr);
                    mainExpr = Expression.MakeBinary(type, mainExpr, currentExpr);
                }
            }
        }

        /* 
         * Term = Factor   (('*'|'/') Factor)*
         *          (*1*)      (*2*)
         */
        private Expression PackTerm()
        {
            Expression factors;
            Expression factor;
            ExpressionType type;

            // Dealing with (*1*).
            if ((factors = PackFactor()) == null)
                return null;

            // Dealing with (*2*).
            while (true)
            {
                switch ((LexiconType)Tokenizer.PeekTokenType())
                {
                    case LexiconType.LSAsterisk:
                        type = ExpressionType.Multiply;
                        Tokenizer.NextToken();
                        break;
                    case LexiconType .LSSlash:
                        type = ExpressionType.Divide;
                        Tokenizer.NextToken();
                        break;
                    default:
                        return factors;
                }

                factor = PackFactor();

                if (factor != null)
                {
                    ImplicitComversion(ref factors,ref factor);
                    factors = Expression.MakeBinary(type, factors, factor);
                }
                else
                    ThrowHelper.ThrowUnexpectedTokenException("Invalid factor term.");
            }
        }

        private Expression PackFactor()
        {
            return PackConstantFactor();
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

        // TODO: This is a simplified implementation. Need to extend as the expansion of the data types.
        private void ImplicitComversion(ref Expression expr1, ref Expression expr2)
        {
            Type type1 = expr1.Type;
            Type type2 = expr2.Type;

            if (type1 != type2)
            {
                if (type1 == typeof(double) && type2 == typeof(int))
                {
                    expr2 = Expression.Convert(expr2, typeof(double));
                }
                else if (type2 == typeof(double) && type1 == typeof(int))
                {
                    expr1 = Expression.Convert(expr1, typeof(double));
                }
            }

        }
    }
}
