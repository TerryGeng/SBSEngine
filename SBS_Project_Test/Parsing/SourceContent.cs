using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBSEngine.Tokenization;

namespace SBSEngine.Parsing
{
    class SourceContent
    {
        private Tokenizer Tokenizer;
        private SourcePosition Position;
        public ParsingError Error { get; private set; }

        public SourceContent(TextReader reader)
        {
            Tokenizer = new Tokenizer(
                new IRule[]{
                new NumberRule(),
                new BlankRule(),
                new NameRule(),
                new SymbolRule(),
                new StringRule(),
                new CommentRule()
                },
                reader
            );

            Position = new SourcePosition();
            Error = new ParsingError(Tokenizer, Position);
        }

        // Functions about operating tokenizer.

        public Token NextToken()
        {
            Token t = Tokenizer.NextToken();

            Position.SetPosition(Tokenizer.Position);
            if (t.Type == (int)LexiconType.LLineBreak)
                Position.AddLine();

            return Tokenizer.NextToken();
        }

        public LexiconType NextTokenType()
        {
            return (LexiconType)NextToken().Type;
        }

        public Token PeekToken()
        {
            return Tokenizer.PeekToken();
        }

        public LexiconType PeekTokenType()
        {
            return (LexiconType)Tokenizer.PeekToken().Type;
        }

        public bool MaybeNext(LexiconType type)
        {
            if (PeekTokenType() == type)
            {
                NextToken();
                return true;
            }

            return false;
        }

        public bool NextTokenType(LexiconType type, string error = null)
        {
            if (!MaybeNext(type))
            {
                Error.ThrowUnexpectedTokenException(error);
                return false;
            }

            return true;
        }

    }
}
