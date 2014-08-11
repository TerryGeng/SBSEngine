using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBSEnvironment.Tokenization;
using SBSEnvironment.Runtime;
using SBSEnvironment.Runtime.Binding;
using SBSEnvironment.Runtime.Binding.Sorter;
using SBSEnvironment.Parsing.Ast;
using SBSEnvironment.Runtime.FuncLibrary;

namespace SBSEnvironment.Parsing
{
    class ParsingContext
    {
        public ParsingError Error { get; private set; }
        public BinaryOpBinder BinaryBinder { get; private set; }
        public BinaryOpSorter BinarySorter { get; private set; }
        public FunctionInvokeBinder FunctionBinder { get; private set; }
        public ExecutableUnit ExecutableUnit;

        private Tokenizer tokenizer;
        private SourcePosition position;

        public ParsingContext(TextReader reader)
        {
            tokenizer = new Tokenizer(
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

            position = new SourcePosition();
            ExecutableUnit = new ExecutableUnit();

            Error = new ParsingError(tokenizer, position);
            BinarySorter = new BinaryOpSorter();
            BinaryBinder = new BinaryOpBinder(BinarySorter);
            FunctionBinder = new FunctionInvokeBinder(ExecutableUnit);

            RegisterOperations();
            RegisterDebugFunctions();

            MaybeNext(LexiconType.LLineBreak); 
        }

        private void RegisterOperations()
        {
            NumericOperations.SelfRegister(BinarySorter);
        }

        private void RegisterDebugFunctions()
        {
            DebugFunctions.LoadFunctions(this.ExecutableUnit);
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
                while((t = tokenizer.NextToken()).Type == (int)LexiconType.LBlank || t.Type == (int)LexiconType.LComment);
            }
            else
            {
                t = _peekToken;
                _peekToken = default(Token);
            }

            position.SetPosition(tokenizer.Position);
            if (t.Type == (int)LexiconType.LLineBreak)
                position.AddLine();

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
