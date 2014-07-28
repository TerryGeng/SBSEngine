using System;
using SBSEnvironment.Tokenization;

namespace SBSEnvironment.Parsing
{
    class ParsingError
    {
        private Tokenizer tokenizer;
        private SourcePosition position;

        public ParsingError(Tokenizer tokenizer,SourcePosition position)
        {
            this.tokenizer = tokenizer;
            this.position = position;
        }

        public void ThrowUnexpectedTokenException(Token token, string message = null)
        {
            throw new UnexpectedTokenException(position.InLinePosition, position.LineNum, token,
                String.Format("Parsing Exception: Unexpected '{0}' on line {1}, position {2}. {3}",
                    ((LexiconType)token.Type).ToString(), position.LineNum, position.InLinePosition, message)
            );
        }
    }

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

}
