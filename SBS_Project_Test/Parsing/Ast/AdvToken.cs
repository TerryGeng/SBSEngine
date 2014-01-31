using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBSEngine.Tokenization;

namespace SBSEngine.Parsing.Ast
{
    /// <summary>
    /// This struct provides some advanced methods to process the Tokenization.Token.
    /// </summary>
    internal struct AdvToken
    {
        public readonly Token BaseToken;

        public LexiconType Type
        {
            get
            {
                return (LexiconType)BaseToken.Type;
            }
        }

        public String Value
        {
            get
            {
                return BaseToken.Value;
            }
        }


        public AbstractTokenType AbstractType { get; private set; }
        public KeywordType KeywordType { get; private set; }

        public AdvToken(Token baseToken)
        {
            BaseToken = baseToken;
            SetAbstractAndKeywordType();
        }

        private void SetAbstractAndKeywordType()
        {
            switch (Type)
            {
                case LexiconType.LInteger:
                case LexiconType.LFloat:
                    AbstractType = AbstractTokenType.Number;
                    break;

                case LexiconType.LName:
                    if (SetKeywordType())
                        AbstractType = AbstractTokenType.Keyword;
                    else
                        AbstractType = AbstractTokenType.Name;

                    break;

                case LexiconType.LSEqual:
                case LexiconType.LSGreater:
                case LexiconType.LSLess:
                case LexiconType.LSPlus:
                case LexiconType.LSMinus:
                case LexiconType.LSAsterisk:
                case LexiconType.LSSlash:
                    AbstractType = AbstractTokenType.Operator;
                    break;

                case LexiconType.LSDollar:
                    AbstractType = AbstractTokenType.VarPrefix;
                    break;

                case LexiconType.LString:
                    AbstractType = AbstractTokenType.String;
                    break;

                case LexiconType.LSLRoundBracket:
                case LexiconType.LSRRoundBracket:
                    AbstractType = AbstractTokenType.Bracket;
                    break;

                case LexiconType.LSComma:
                    AbstractType = AbstractTokenType.Comma;
                    break;

                case LexiconType.LLineBreak:
                    AbstractType = AbstractTokenType.LineBreak;
                    break;
            }
        }

        private bool SetKeywordType()
        {
            switch (Value.ToLower())
            {
                case "if": KeywordType = KeywordType.KeywordIf; return true;
                case "else": KeywordType = KeywordType.KeywordElse; return true;
                case "for": KeywordType = KeywordType.KeywordFor; return true;
                case "while": KeywordType = KeywordType.KeywordWhile; return true;
                default: return false;
            }
        }
    }

    internal enum AbstractTokenType
    {
        Null,

        Operator,
        Keyword,
        Number,
        Name,
        String,
        VarPrefix,
        Bracket,
        Comma,
        LineBreak
    }

    internal enum KeywordType
    {
        Null,

        KeywordIf,
        KeywordElse,
        KeywordFor,
        KeywordWhile
    }
}
