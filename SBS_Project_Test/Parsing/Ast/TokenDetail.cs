using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBSEngine.Tokenization;

namespace SBSEngine.Parsing.Ast
{
    /// <summary>
    /// This struct provides some advanced methods to process the Token.
    /// </summary>
    internal struct TokenDetail
    {
        public readonly Token BaseToken;
        public readonly LexiconType Type;
        public readonly String Value;
        public readonly AbstractTokenType AbstractType;
        public readonly KeywordType KeywordType;

        public TokenDetail(Token baseToken)
            : this()
        {
            BaseToken = baseToken;
            Type = (LexiconType)BaseToken.Type;
            Value = BaseToken.Value;

            switch (Type)
            {
                case LexiconType.LInteger:
                case LexiconType.LFloat:
                    AbstractType = AbstractTokenType.Number;
                    return;

                case LexiconType.LName:
                    switch (Value.ToLower())
                    {
                        case "if": KeywordType = KeywordType.KeywordIf; break;
                        case "else": KeywordType = KeywordType.KeywordElse; break;
                        case "for": KeywordType = KeywordType.KeywordFor; break;
                        case "while": KeywordType = KeywordType.KeywordWhile; break;
                        default: AbstractType = AbstractTokenType.Name; 
                            return;
                    }
                    AbstractType = AbstractTokenType.Keyword;
                    return;

                case LexiconType.LSEqual:
                case LexiconType.LSGreater:
                case LexiconType.LSLess:
                case LexiconType.LSPlus:
                case LexiconType.LSMinus:
                case LexiconType.LSAsterisk:
                case LexiconType.LSSlash:
                    AbstractType = AbstractTokenType.Operator;
                    return;

                case LexiconType.LSDollar:
                    AbstractType = AbstractTokenType.VarPrefix;
                    return;

                case LexiconType.LString:
                    AbstractType = AbstractTokenType.String;
                    return;

                case LexiconType.LSLRoundBracket:
                case LexiconType.LSRRoundBracket:
                    AbstractType = AbstractTokenType.Bracket;
                    return;

                case LexiconType.LSComma:
                    AbstractType = AbstractTokenType.Comma;
                    return;

                case LexiconType.LLineBreak:
                    AbstractType = AbstractTokenType.LineBreak;
                    return;
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
