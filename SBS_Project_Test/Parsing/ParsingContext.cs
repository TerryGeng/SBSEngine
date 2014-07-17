﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBSEngine.Tokenization;
using SBSEngine.Runtime;
using SBSEngine.Runtime.Binding;
using SBSEngine.Runtime.Binding.Sorter;
using SBSEngine.Parsing.Ast;

namespace SBSEngine.Parsing
{
    class ParsingContext
    {
        private Tokenizer Tokenizer;
        private SourcePosition Position;
        public ParsingError Error { get; private set; }
        public BinaryOpBinder BinaryBinder;
        public BinaryOpSorter BinarySorter;

        public ParsingContext(TextReader reader)
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
            BinarySorter = new BinaryOpSorter();
            RegisterOpSorter();
            BinaryBinder = new BinaryOpBinder(BinarySorter);

            MaybeNext(LexiconType.LLineBreak); 
        }

        private void RegisterOpSorter()
        {
            NumericOpSorter.SelfRegister(BinarySorter);
        }

        /*
         * Functions about operating tokenizer.
         */
        #region Tokenizer Operating

        private Token _peekToken;

        public Token NextToken()
        {
            Token t;
            if (_peekToken.Type == 0)
            {
                while((t = Tokenizer.NextToken()).Type == (int)LexiconType.LBlank || t.Type == (int)LexiconType.LComment);
            }
            else
            {
                t = _peekToken;
                _peekToken = default(Token);
            }

            Position.SetPosition(Tokenizer.Position);
            if (t.Type == (int)LexiconType.LLineBreak)
                Position.AddLine();

            return t;
        }

        public Token NextToken(LexiconType type,string error = null)
        {
            Token t = NextToken();

            if(t.Type != (int)type)
                Error.ThrowUnexpectedTokenException(PeekToken(), error);

            return t;
        }

        public Token PeekToken()
        {
            if (_peekToken.Type == 0)
                _peekToken = NextToken();
            return _peekToken;
        }

        public bool PeekToken(LexiconType type)
        {
            if (PeekTokenType() == type)
                return true;
            return false;
        }

        public LexiconType NextTokenType()
        {
            return (LexiconType)NextToken().Type;
        }

        public LexiconType PeekTokenType()
        {
            return (LexiconType)PeekToken().Type;
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
                Error.ThrowUnexpectedTokenException(PeekToken(), error);
                return false;
            }

            return true;
        }
        #endregion


    }
}
