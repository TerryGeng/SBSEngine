using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBSEngine.Tokenization;
using SBSEngine.Parsing.ExprStatment;
using SBSEngine.Runtime.Types;

namespace SBSEngine.Parsing
{
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
        private static ExpressionPacker instance;

        public Tokenizer Tokenizer { set; private get; }

        //public IPacker Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //            instance = new ExpressionPacker();
        //        return instance;
        //    }
        //}

        public IStatment PackStatment()
        {
            return null;
        }

        public Expression PackExpression() // TODO: public(for debug) => private
        {
            // Expression= Factor*

            Expression expr = new Expression();
            Factor factor;

            factor = PackFactor(true);

            if (factor == null)
                throw new ApplicationException("No factors at all."); // TODO

            expr.AddFactor(factor);

            while (true)
            {
                factor = PackFactor();
                if (factor == null)
                    break;

                expr.AddFactor(factor);
            }

            return expr;
        }

        private Factor PackFactor(bool firstFactor = false)
        {
            // Factor = ('+'|'-') Term*

            Factor factor = new Factor();
            Token token;

            // Parsing op.
            if (firstFactor)
            {
                factor.Op = FactorOp.Plus;
            }
            else
            {
                token = Tokenizer.PeekToken();

                switch (token.Type)
                {
                    case (int)LexiconType.LSPlus:
                        factor.Op = FactorOp.Plus;
                        break;
                    case (int)LexiconType.LSMinus:
                        factor.Op = FactorOp.Minus;
                        break;
                    default:
                        return null;
                }

                Tokenizer.NextToken();
            }


            // Parsing object that be operated.
            ITerm term;

            term = PackConstantTerm(true);

            if (term == null)
                throw new ApplicationException("Only term op."); // TODO

            factor.AddTerm(term);

            while (true)
            {
                term = PackConstantTerm(); // TODO: pack multiple terms besides constant.

                if (term == null)
                    break;
                else
                    factor.AddTerm(term);
            }

            return factor;
        }

        private ConstantTerm PackConstantTerm(bool firstTerm = false)
        {
            // Term = ('*'|'/') (<Integer>|<Double>|...TODO...)

            IConstValue value;
            TermOp op;

            Token token;

            if (firstTerm)
            {
                op = TermOp.None;
            }
            else
            {
                token = Tokenizer.PeekToken();

                switch (token.Type)
                {
                    case (int)LexiconType.LSAsterisk:
                        op = TermOp.Mul;
                        break;
                    case (int)LexiconType.LSSlash:
                        op = TermOp.Div;
                        break;
                    default:
                        return null;
                }

                Tokenizer.NextToken();
            }

            token = Tokenizer.PeekToken();

            switch (token.Type)
            {
                case (int)LexiconType.LInteger:
                    value = new IntegerValue(Int32.Parse(token.Value));
                    break;
                case (int)LexiconType.LFloat:
                    value = new DoubleValue(Double.Parse(token.Value));
                    break;
                default:
                    throw new ApplicationException("Only term op?"); // TODO
            }

            Tokenizer.NextToken();

            return new ConstantTerm(op, value);
        }
    }
}
