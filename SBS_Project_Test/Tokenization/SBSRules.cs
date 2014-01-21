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
        LSGreater,
        LSLess,
        LSDot,

        LComment
    }

    public class NumberRule : IRule
    {
        const int INT = 0;
        const int DOT = 1;
        const int FLOAT = 2;
        int status = INT;

        ScannerResult IRule.Scan(int character)
        {
            switch (status)
            {
                case INT:
                    if (char.IsDigit((char)character))
                    {
                        return new ScannerResult{ Result = ScannerStatus.Continued };
                    }
                    else if ((char)character == '.')
                    {
                        status = DOT;
                        return new ScannerResult { Result = ScannerStatus.Continued };
                    }
                    break;
                case DOT:
                    if (char.IsDigit((char)character))
                    {
                        status = FLOAT;
                        return new ScannerResult { Result = ScannerStatus.Continued };
                    }
                    break;
                case FLOAT:
                    if (char.IsDigit((char)character))
                    {
                        return new ScannerResult { Result = ScannerStatus.Continued };
                    }
                    break;
            }

            if (status != DOT)
                return new ScannerResult { Result = ScannerStatus.PreviousFinished };
            else
                return new ScannerResult { Result = ScannerStatus.Unmatch };
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
        ReadingOption option = ReadingOption.IgnoreCurrent;

        ScannerResult IRule.Scan(int character)
        {
            if (char.IsWhiteSpace((char)character))
            {
                if (character == '\n')
                {
                    status = LINEBREAK;
                    option = ReadingOption.Normal;
                }
                return new ScannerResult { Result = ScannerStatus.Continued };
            }

            return new ScannerResult { Result = ScannerStatus.PreviousFinished ,Option = option };
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
            option = ReadingOption.IgnoreCurrent;
        }
    }

    public class NameRule : IRule
    {
        const int FIRST = 0;
        const int AFTER = 1;

        int status = 0;

        ScannerResult IRule.Scan(int character)
        {
            if (status == FIRST && (char.IsLetter((char)character) || character == '_'))
            {
                status = AFTER;
                return new ScannerResult { Result = ScannerStatus.Continued  };
            }
            else if (status == AFTER && (char.IsLetterOrDigit((char)character) || character == '_'))
            {
                return new ScannerResult { Result = ScannerStatus.Continued };
            }

            return new ScannerResult { Result = ScannerStatus.PreviousFinished };
        }

        Token IRule.Pack(StringBuilder buffer)
        {
            return new Token
            {
                Type = (int)LexiconType.LName,
                Value = buffer.ToString()
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

        ScannerResult IRule.Scan(int character)
        {
            switch (character)
            {
                case '(':
                    type = LexiconType.LSLRoundBracket;
                    return new ScannerResult { Result = ScannerStatus.Finished };
                case ')':
                    type = LexiconType.LSRRoundBracket;
                    return new ScannerResult { Result = ScannerStatus.Finished };
                case '$':
                    type = LexiconType.LSDollar;
                    return new ScannerResult { Result = ScannerStatus.Finished };
                case ',':
                    type = LexiconType.LSComma;
                    return new ScannerResult { Result = ScannerStatus.Finished };
                case '+':
                    type = LexiconType.LSPlus;
                    return new ScannerResult { Result = ScannerStatus.Finished };
                case '-':
                    type = LexiconType.LSMinus;
                    return new ScannerResult { Result = ScannerStatus.Finished };
                case '*':
                    type = LexiconType.LSAsterisk;
                    return new ScannerResult { Result = ScannerStatus.Finished };
                case '/':
                    type = LexiconType.LSSlash;
                    return new ScannerResult { Result = ScannerStatus.Finished };
                case '=':
                    type = LexiconType.LSEqual;
                    return new ScannerResult { Result = ScannerStatus.Finished };
                case '>':
                    type = LexiconType.LSGreater;
                    return new ScannerResult { Result = ScannerStatus.Finished };
                case '<':
                    type = LexiconType.LSLess;
                    return new ScannerResult { Result = ScannerStatus.Finished };
                case '.':
                    type = LexiconType.LSDot;
                    return new ScannerResult { Result = ScannerStatus.Finished };
                default:
                    return new ScannerResult { Result = ScannerStatus.Unmatch };
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
        }
    }

    public class StringRule : IRule
    {
        bool firstChar = true;
        bool backslash = false;

        ScannerResult IRule.Scan(int character)
        {
            if (firstChar)
            {
                if (character == (int)'"')
                {
                    firstChar = false;
                    return new ScannerResult { Result = ScannerStatus.Continued };
                }

                return new ScannerResult { Result = ScannerStatus.Unmatch };
            }
            else
            {
                switch ((char)character)
                {
                    case '\\':
                        backslash = true;
                        return new ScannerResult { Result = ScannerStatus.Continued };
                    case '"':
                        if (!backslash)
                            return new ScannerResult { Result = ScannerStatus.Finished };
                        break;
                }

                backslash = false;
                return new ScannerResult { Result = ScannerStatus.Continued };
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

        ScannerResult IRule.Scan(int character)
        {
            if (firstChar)
            {
                if (character == (int)'\'')
                {
                    firstChar = false;
                    return new ScannerResult { Result = ScannerStatus.Continued };
                }
            }
            else
            {
                if (character == (int)'\n')
                {
                    return new ScannerResult { Result = ScannerStatus.PreviousFinished , Option = ReadingOption.IgnoreCurrent};
                }
                return new ScannerResult { Result = ScannerStatus.Continued };
            }

            return new ScannerResult { Result = ScannerStatus.Unmatch };
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
