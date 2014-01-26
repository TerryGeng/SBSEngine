namespace SBSEngine.Tests
{
    using System.Windows.Forms;
    using SBSEngine.Tokenization;
    using System.Diagnostics;
    using System.Collections.Generic;
    using SBSEngine.Parsing;
    using System.Linq.Expressions;
    using System;

    public partial class TestForm : Form
    {
        List<IRule> rules = new List<IRule>();
        Tokenizer Tokenizer;

        public TestForm()
        {
            InitializeComponent();
            rules.Add(new NumberRule());
            rules.Add(new BlankRule());
            rules.Add(new NameRule());
            rules.Add(new SymbolRule());
            rules.Add(new StringRule());
            rules.Add(new CommentRule());

        }

        public void emptyFunc(string str) { }

        private void button1_Click_1(object sender, System.EventArgs e)
        {
            this.textBox2.Clear();
            ExpressionPackTest();
        }

        private void TokenizerTest()
        {
            Tokenizer = new Tokenizer(rules, textBox1.Text);
            Token Token;
            int TokenCount = 0;
            Stopwatch Watch = Stopwatch.StartNew();

            try
            {
                if (checkBox1.Checked)
                {
                    while (true)
                    {
                        Token = Tokenizer.NextToken();
                        if (Token.Type == (int)LexiconType.Null)
                            break;
                        textBox2.AppendText("Token: " + ((SBSEngine.Tokenization.LexiconType)Token.Type).ToString() + " " + Token.Value + "\r\n");
                        TokenCount += 1;
                    }
                }
                else
                {
                    while (true)
                    {
                        Token = Tokenizer.NextToken();
                        if (Token.Type == (int)LexiconType.Null)
                            break;
                        TokenCount += 1;
                    }
                }
            }
            catch (UnexpectedCharacterException ex)
            {
                textBox2.AppendText(string.Format("Error: {0}", ex.Message) + "\r\n");
            }

            Watch.Stop();

            textBox2.AppendText(string.Format("Processed {0:d} token(s).", TokenCount) + "\r\n");
            textBox2.AppendText(string.Format("Elapsed: {0:d}ms.", Watch.ElapsedMilliseconds) + "\r\n");
        }

        private void ExpressionPackTest()
        {
            Tokenizer = new Tokenizer(rules, textBox1.Text);
            ExpressionPacker exprPacker = new ExpressionPacker();
            exprPacker.Tokenizer = Tokenizer;

            try
            {
                // Packing
                textBox2.AppendText("Packing Code... ");
                Stopwatch Watch = Stopwatch.StartNew();
                Expression expr = exprPacker.PackExpression();
                Watch.Stop();
                textBox2.AppendText("Done. ");
                textBox2.AppendText(string.Format("Elapsed: {0:d}ms.", Watch.ElapsedMilliseconds) + "\r\n\r\n");
                if (checkBox1.Checked)
                    textBox2.AppendText("Packing Result: " + expr.ToString() + "\r\n\r\n");

                // Compiling
                Func<double> code;
                textBox2.AppendText("Compiling... ");
                Watch = Stopwatch.StartNew();
                code = Expression.Lambda<Func<double>>(Expression.Convert(expr,typeof(double))).Compile();
                Watch.Stop();
                textBox2.AppendText("Done. ");
                textBox2.AppendText(string.Format("Elapsed: {0:d}ms.", Watch.ElapsedMilliseconds) + "\r\n\r\n");

                // Executing
                double result;
                textBox2.AppendText("Executing... ");
                Watch = Stopwatch.StartNew();
                result = code();
                Watch.Stop();

                textBox2.AppendText("Done. ");
                textBox2.AppendText(string.Format("Result: {0}. ",result));
                textBox2.AppendText(string.Format("Elapsed: {0:d}ms.", Watch.ElapsedMilliseconds) + "\r\n\r\n");
            }
            catch (ApplicationException ex)
            {
                textBox2.AppendText(string.Format("\r\nError: {0} \r\n\r\n", ex.Message));
            }


        }

    }
}
