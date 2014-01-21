using System;
using System.Collections.Generic;
using System.Text;
using SBSEngine.Tokenization;
using SBSEngine.Runtime.Types;

namespace SBSEngine.Parsing.ExprStatment
{
    public enum FactorOp
    {
        Undefined,
        Plus,
        Minus
    }

    public enum TermOp
    {
        Undefined,
        None,
        Mul,
        Div
    }

    public interface IExprVisitor 
    {
        void Visit(Expression expr);
        void Visit(Factor factor);
        void Visit(ConstantTerm constTerm);
    }

    public interface IExprElement
    {
        void Accept(IExprVisitor visitor);
    }

    public interface ITerm : IExprElement
    {
        TermOp Op { get; set; }
    }

    public class ConstantTerm : ITerm
    {
        public IConstValue Value { get; set; }
        public TermOp Op { get; set; }

        public void Accept(IExprVisitor visitor)
        {
            visitor.Visit(this);
        }

        public ConstantTerm(TermOp op,IConstValue value)
        {
            Op = op;
            Value = value;
        }
    }

    //public class VariableTerm : ITerm
    //{
    //    public TermOp Op;
    //    public void Accept() { }
    //    public IConstValue value;
    //}

    public class Factor : IExprElement
    {
        public FactorOp Op { get; set; }
        public List<ITerm> Terms { get; private set; }

        public Factor()
        {
            Terms = new List<ITerm>();
        }

        public int TermsCount
        {
            get{ return Terms.Count; }
        }

        public void Accept(IExprVisitor visitor)
        {
            visitor.Visit(this);
        }
        public void AddTerm(ITerm term)
        {
            Terms.Add(term);
        }
    }

    public class Expression : ITerm
    {
        public TermOp Op { set; get; }
        public List<Factor> Factors { get; private set; }

        public Expression()
        {
            Factors = new List<Factor>();
        }

        public void Accept(IExprVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void AddFactor(Factor factor)
        {
            Factors.Add(factor);
        }
    }

    //public class CallExpression : ITerm
    //{

    //}

    //public class ExpressionStatment : IStatment
    //{
    //    public Expression Expression;
    //    public void Accpet()
    //    {

    //    }
    //}

    public class TestExprVisior : IExprVisitor
    {
        public delegate void Output(string str);

        Output output;
        int level = 0;

        public TestExprVisior(Output output)
        {
            this.output = output;
        }

        private void indentedOutput(string str)
        {
            for (int i = 0; i < level; ++i)
            {
                output(" ");
            }
            output(str);
        }

        public void Visit(Expression expr)
        {
            ++level;
            indentedOutput(string.Format("Expression ({0:d})\r\n",expr.Factors.Count));
            foreach (Factor factor in expr.Factors)
            {
                factor.Accept(this);
            }
            --level;
        }

        public void Visit(Factor factor)
        {
            ++level;
            indentedOutput(string.Format("Factor ({0:d})\r\n", factor.Terms.Count));
            foreach (ITerm term in factor.Terms)
            {
                term.Accept(this);
            }
            --level;
        }

        public void Visit(ConstantTerm constTerm)
        {
            ++level;
            indentedOutput(string.Format("ConstantTerm: {0:d}. \r\n", constTerm.Value.ToString()));
            --level;
        }
    }
}
