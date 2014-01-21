namespace Tokenizer_debug
{
    using System.Windows.Forms;
    using SBSEngine.Tokenization;
    using System.Diagnostics;
    using System.Collections.Generic;
    using SBSEngine.Parsing;
    using SBSEngine.Parsing.ExprStatment;

    public partial class Form1 : Form
    {
        List<IRule> rules = new List<IRule>();
        Tokenizer Tokenizer;

        public Form1()
        {
            InitializeComponent();
            rules.Add(new NumberRule());
            rules.Add(new BlankRule());
            rules.Add(new NameRule());
            rules.Add(new SymbolRule());
            rules.Add(new StringRule());
            rules.Add(new CommentRule());

        }

        public void emptyFunc(string str){}

        private void button1_Click_1(object sender, System.EventArgs e)
        {
            this.textBox2.Clear();
            Tokenizer = new Tokenizer(rules, textBox1.Text);
            ExpressionPacker exprPacker = new ExpressionPacker();
            exprPacker.Tokenizer = Tokenizer;

            Stopwatch Watch = Stopwatch.StartNew();

            Expression expr = exprPacker.PackExpression();
            TestExprVisior testVisitor;

            if (checkBox1.Checked)
            {
                testVisitor = new TestExprVisior(textBox2.AppendText);
            }
            else
            {
                testVisitor = new TestExprVisior(emptyFunc);
            }
            
            testVisitor.Visit(expr);
            textBox2.AppendText("Done. \r\n");

            Watch.Stop();

            textBox2.AppendText(string.Format("Elapsed: {0:d}ms.", Watch.ElapsedMilliseconds) + "\r\n");

        }

        private void TokenizerTest()
        {
            Tokenizer = new Tokenizer(rules, textBox1.Text);
            this.textBox2.Clear();
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

    }
}
