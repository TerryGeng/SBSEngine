using System.Text;

namespace SBSEngine.Tokenization
{
    public enum LexiconType
    {
        Null,

        LInteger,
        LFloat,
        LName,
        LBlank,
        LLineBreak,
        LString,
        LSymbol,

        LSLRoundBracket,
        LSRRoundBracket,
        LSDollar,
        LSComma,
        LSPlus,
        LSMinus,
        LSAsterisk,
        LSSlash,
        LSEqual,
        LSDoubleEqual,
        LSGreater,
        LSGreaterEqual,
        LSLess,
        LSLessEqual,
        LSPlusEqual,
        LSLessGreater,
        LSDot,

        LKIf,
        LKThen,
        LKElse,
        LKFor,
        LKWhile,
        LKEnd,
        LKNext,

        LComment
    }

    public class NumberRule : IRule
    {
        const int INT = 0;
        const int DOT = 1;
        const int FLOAT = 2;
        int status = INT;

        ScannerStatus IRule.Scan(int character)
        {
            switch (status)
            {
                case INT:
                    if (char.IsDigit((char)character))
                    {
                        return ScannerStatus.Continued;
                    }
                    else if ((char)character == '.')
                    {
                        status = DOT;
                        return ScannerStatus.Continued ;
                    }
                    break;
                case DOT:
                    if (char.IsDigit((char)character))
                    {
                        status = FLOAT;
                        return ScannerStatus.Continued;
                    }
                    break;
                case FLOAT:
                    if (char.IsDigit((char)character))
                    {
                        return ScannerStatus.Continued;
                    }
                    break;
            }

            if (status != DOT)
                return ScannerStatus.PreviousFinished;
            else
                return ScannerStatus.Unmatch;
        }

        Token IRule.Pack(StringBuilder buffer)
        {
            switch (status)
            {
                case INT:
                    return new Token
                    {
                        Type = (int)LexiconType.LInteger,
                        Value = buffer.ToString()
                    };
                case FLOAT:
                    return new Token
                    {
                        Type = (int)LexiconType.LFloat,
                        Value = buffer.ToString()
                    };
                default:
                    return new Token();
            }
        }

        void IRule.Reset()
        {
            status = INT;
        }

    }

    public class BlankRule : IRule
    {
        const int SPACE = 0;
        const int LINEBREAK = 1;
        int status = SPACE;

        ScannerStatus IRule.Scan(int character)
        {
            if (char.IsWhiteSpace((char)character))
            {
                if (character == '\n')
                {
                    status = LINEBREAK;
                }
                return ScannerStatus.Continued;
            }

            return ScannerStatus.PreviousFinished;
        }

        Token IRule.Pack(StringBuilder buffer)
        {
            switch (status)
            {
                case SPACE:
                    return new Token
                    {
                        Type = (int)LexiconType.LBlank,
                        Value = null
                    };
                case LINEBREAK:
                    return new Token
                    {
                        Type = (int)LexiconType.LLineBreak,
                        Value = null
                    };
                default:
                    return new Token();
            }
        }

        void IRule.Reset() 
        {
            status = SPACE;
        }
    }

    public class NameRule : IRule
    {
        const int FIRST = 0;
        const int AFTER = 1;

        int status = 0;

        ScannerStatus IRule.Scan(int character)
        {
            if (status == FIRST && (char.IsLetter((char)character) || character == '_'))
            {
                status = AFTER;
                return ScannerStatus.Continued;
            }
            else if (status == AFTER && (char.IsLetterOrDigit((char)character) || character == '_'))
            {
                return ScannerStatus.Continued;
            }

            return ScannerStatus.PreviousFinished;
        }

        Token IRule.Pack(StringBuilder buffer)
        {
            string value = buffer.ToString();
            int type = (int)LexiconType.LName;

            // Dealing with keywords.
            if (value.Length <= 5) // Note: Avoid trying to compare a long string(longer than the longest keyword).
            {
                if (string.Compare(value, "if", true) == 0)
                {
                    value = null;
                    type = (int)LexiconType.LKIf;
                }
                else if (string.Compare(value, "then", true) == 0)
                {
                    value = null;
                    type = (int)LexiconType.LKThen;
                }
                else if (string.Compare(value, "else", true) == 0)
                {
                    value = null;
                    type = (int)LexiconType.LKElse;
                }
                else if (string.Compare(value, "for", true) == 0)
                {
                    value = null;
                    type = (int)LexiconType.LKFor;
                }
                else if (string.Compare(value, "while", true) == 0)
                {
                    value = null;
                    type = (int)LexiconType.LKWhile;
                }
                else if (string.Compare(value, "end", true) == 0)
                {
                    value = null;
                    type = (int)LexiconType.LKEnd;
                }
                else if (string.Compare(value, "next", true) == 0)
                {
                    value = null;
                    type = (int)LexiconType.LKNext;
                }

                
            }


            return new Token
            {
                Type = type,
                Value = value
            };
        }

        void IRule.Reset()
        {
            status = FIRST;
        }
    }

    public class SymbolRule : IRule
    {
        LexiconType type = LexiconType.Null;
        int status = 0;
        /* 1: While meeting '+', maybe next is '='.
         * 2: '=' , '='
         */

        ScannerStatus IRule.Scan(int character)
        {
            if (status == 0)
            {
                switch (character)
                {
                    case '+':
                        type = LexiconType.LSPlus;
                        status = 1;
                        return ScannerStatus.Continued;

                    case '(':
                        type = LexiconType.LSLRoundBracket;
                        return ScannerStatus.Finished;
                    case ')':
                        type = LexiconType.LSRRoundBracket;
                        return ScannerStatus.Finished;
                    case '$':
                        type = LexiconType.LSDollar;
                        return ScannerStatus.Finished;
                    case ',':
                        type = LexiconType.LSComma;
                        return ScannerStatus.Finished;
                    case '-':
                        type = LexiconType.LSMinus;
                        return ScannerStatus.Finished;
                    case '*':
                        type = LexiconType.LSAsterisk;
                        return ScannerStatus.Finished;
                    case '/':
                        type = LexiconType.LSSlash;
                        return ScannerStatus.Finished;
                    case '=':
                        type = LexiconType.LSEqual;
                        status = 1;
                        return ScannerStatus.Continued;

                    case '>':
                        type = LexiconType.LSGreater;
                        status = 1;
                        return ScannerStatus.Finished;
                    case '<':
                        type = LexiconType.LSLess;
                        status = 1;
                        return ScannerStatus.Finished;
                    case '.':
                        type = LexiconType.LSDot;
                        return ScannerStatus.Finished;
                    default:
                        return ScannerStatus.Unmatch;
                }
            }
            else
            {
                if (type == LexiconType.LSPlus && (char)character == '=')
                {
                    type = LexiconType.LSPlusEqual;
                    return ScannerStatus.Finished;
                }
                else if (type == LexiconType.LSEqual && (char)character == '=')
                {
                    type = LexiconType.LSDoubleEqual;
                    return ScannerStatus.Finished;
                }
                else if (type == LexiconType.LSGreater && (char)character == '=')
                {
                    type = LexiconType.LSGreaterEqual;
                    return ScannerStatus.Finished;
                }
                else if (type == LexiconType.LSLess)
                {
                    if ((char)character == '=')
                    {
                        type = LexiconType.LSLessEqual;
                        return ScannerStatus.Finished;
                    }
                    else if ((char)character == '>')
                    {
                        type = LexiconType.LSLessGreater;
                        return ScannerStatus.Finished;
                    }
                }
                return ScannerStatus.PreviousFinished;
            }
        }

        Token IRule.Pack(StringBuilder buffer)
        {
            return new Token
            {
                Type = (int)type,
                Value = null
            };
        }

        void IRule.Reset()
        {
            type = LexiconType.Null;
            status = 0;
        }
    }

    public class StringRule : IRule
    {
        bool firstChar = true;
        bool backslash = false;

        ScannerStatus IRule.Scan(int character)
        {
            if (firstChar)
            {
                if (character == (int)'"')
                {
                    firstChar = false;
                    return ScannerStatus.Continued;
                }

                return ScannerStatus.Unmatch;
            }
            else
            {
                switch ((char)character)
                {
                    case '\\':
                        backslash = true;
                        return ScannerStatus.Continued;
                    case '"':
                        if (!backslash)
                            return ScannerStatus.Finished;
                        break;
                }

                backslash = false;
                return ScannerStatus.Continued;
            }
        }

        Token IRule.Pack(StringBuilder buffer)
        {
            return new Token
            {
                Type = (int)LexiconType.LString,
                Value = buffer.ToString(1, buffer.Length - 3)
            };
        }

        void IRule.Reset()
        {
            firstChar = true;
            backslash = false;
        }
    }

    public class CommentRule : IRule
    {
        bool firstChar = true;

        ScannerStatus IRule.Scan(int character)
        {
            if (firstChar)
            {
                if (character == (int)'\'')
                {
                    firstChar = false;
                    return ScannerStatus.Continued;
                }
            }
            else
            {
                if (character == (int)'\n')
                {
                    return ScannerStatus.PreviousFinished;
                }
                return ScannerStatus.Continued;
            }

            return ScannerStatus.Unmatch;
        }

        Token IRule.Pack(StringBuilder buffer)
        {
            return new Token
            {
                Type = (int)LexiconType.LComment,
                Value = null
            };
        }

        void IRule.Reset()
        {
            firstChar = true;
        }
    }

}
