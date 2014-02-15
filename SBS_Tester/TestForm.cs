namespace SBSEngine.Tests
{
    using System.Windows.Forms;
    using SBSEngine.Tokenization;
    using System.Diagnostics;
    using System.Collections.Generic;
    using SBSEngine.Parsing;
    using System.Linq.Expressions;
    using System.IO;
    using System;

    public partial class TestForm : Form
    {
        Parser parser;

        public TestForm()
        {
            InitializeComponent();
        }

        public void emptyFunc(string str) { }

        private void button1_Click_1(object sender, System.EventArgs e)
        {
            this.textBox2.Clear();


            if (ifCatch.Checked)
            {
                try
                {
                    ExpressionPackTest();
                }
                catch (ApplicationException ex)
                {
                    textBox2.AppendText(string.Format("\r\nError: {0} \r\n\r\n", ex.Message));
                }
            }
            else
            {
                ExpressionPackTest();
            }

            //if (ifCatch.Checked)
            //{
            //    try
            //    {
            //        TokenizerTest();
            //    }
            //    catch (ApplicationException ex)
            //    {
            //        textBox2.AppendText(string.Format("\r\nError: {0} \r\n\r\n", ex.Message));
            //    }
            //}
            //else
            //{
            //    TokenizerTest();
            //}
        }

        private void TokenizerTest()
        {
            Tokenizer tokenizer;
            tokenizer = new Tokenizer(
                new IRule[]{
                new NumberRule(),
                new BlankRule(),
                new NameRule(),
                new SymbolRule(),
                new StringRule(),
                new CommentRule()
                },
                new StringReader(textBox1.Text)
            );

            Token Token;
            int TokenCount = 0;
            Stopwatch Watch = Stopwatch.StartNew();

            if (ifDisplay.Checked)
            {
                while (true)
                {
                    Token = tokenizer.NextToken();
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
                    Token = tokenizer.NextToken();
                    if (Token.Type == (int)LexiconType.Null)
                        break;
                    TokenCount += 1;
                }
            }


            Watch.Stop();

            textBox2.AppendText(string.Format("Processed {0:d} token(s).", TokenCount) + "\r\n");
            textBox2.AppendText(string.Format("Elapsed: {0:d}ms.", Watch.ElapsedMilliseconds) + "\r\n");
        }

        private void ExpressionPackTest()
        {
            parser = Parser.CreateParserFromString(textBox1.Text);

            // Packing
            textBox2.AppendText("Packing Code... ");
            Stopwatch Watch = Stopwatch.StartNew();
            Expression expr = parser.Parse();
            Watch.Stop();
            textBox2.AppendText("Done. ");
            textBox2.AppendText(string.Format("Elapsed: {0:d}ms.", Watch.ElapsedMilliseconds) + "\r\n\r\n");
            if (ifDisplay.Checked)
                textBox2.AppendText("Packing Result: " + expr.ToString() + "\r\n\r\n");

            // Compiling
            Func<object> code;
            textBox2.AppendText("Compiling... ");
            Watch = Stopwatch.StartNew();
            code = Expression.Lambda<Func<object>>(expr).Compile();
            Watch.Stop();
            textBox2.AppendText("Done. ");
            textBox2.AppendText(string.Format("Elapsed: {0:d}ms.", Watch.ElapsedMilliseconds) + "\r\n\r\n");

            // Executing
            object result;
            textBox2.AppendText("Executing... ");
            Watch = Stopwatch.StartNew();
            result = code();
            Watch.Stop();

            textBox2.AppendText("Done. ");
            textBox2.AppendText(string.Format("Result: {0}. ", result));
            textBox2.AppendText(string.Format("Elapsed: {0:d}ms.", Watch.ElapsedMilliseconds) + "\r\n\r\n");

        }

    }
}
