using System;
using System.Collections;
using System.Text;
using LexiconRules = System.Collections.Generic.Dictionary<Tokenizer.TokenizerRules.LexiconType, Tokenizer.TokenizerRules.LexiconRule>;

namespace Tokenizer
{
    public struct Token
    {
        public TokenizerRules.LexiconType Type;
        public string Value;
    }

    static public class TokenizerRules
    {
        public class LexiconRule
        {
            public delegate ScannerStatus ScannerFunction(char Character, int Position);
            public LexiconType Type;

            public ScannerFunction Scan;
            public LexiconRule(LexiconType Type, ScannerFunction Scanner)
            {
                this.Type = Type;
                this.Scan = Scanner;
            }
        }

        public enum LexiconType
        {
            Undefined,
            LInteger,
            LFloat,
            LName,
            LString,
            LBlank,
            LCrLf,
            LSymbol
        }

        public enum ScannerStatus
        {
            /// <summary>
            /// Current and previous characters still match the rule. Match process continue.
            /// </summary>
            /// <remarks></remarks>
            Continued,
            /// <summary>
            /// The emergence of current char indicated this combination unmatched the rule.
            /// </summary>
            /// <remarks></remarks>
            Unmatch,
            /// <summary>
            /// Current and previous characters matchs the rule. Match process end.
            /// </summary>
            /// <remarks></remarks>
            Finished,
            /// <summary>
            /// Only previous characters match the rule. Match process end.
            /// </summary>
            /// <remarks></remarks>
            PreviousFinished

        }

        public class UnexpectedCharacterException : ApplicationException
        {

            public int Position;

            public char Character;
            public UnexpectedCharacterException(int Position, char Character)
                : base(string.Format("Unexpected Character '{0}'({1}) at {2}.", Character, Convert.ToInt32(Character), Position))
            {
                this.Position = Position;
                this.Character = Character;
            }
        }

        public static void LoadLexicalRules(LexiconRules Container)
        {
            Container.Add(LexiconType.LInteger, new LexiconRule(LexiconType.LInteger, IntegerScanner));
            Container.Add(LexiconType.LBlank, new LexiconRule(LexiconType.LBlank, BlankScanner));
            Container.Add(LexiconType.LName, new LexiconRule(LexiconType.LName, NameScanner));
            Container.Add(LexiconType.LCrLf, new LexiconRule(LexiconType.LCrLf, CrLfScanner));

        }

        public static ScannerStatus IntegerScanner(char Character, int Position)
        {
            if (char.IsDigit(Character))
                return ScannerStatus.Continued;
            return ScannerStatus.PreviousFinished;
        }

        public static ScannerStatus BlankScanner(char Character, int Position)
        {
            // Character is neither vbCr nor vbLf
            if ((!char.IsControl(Character)) && char.IsWhiteSpace(Character))
                return ScannerStatus.Continued;
            return ScannerStatus.PreviousFinished;
        }

        public static ScannerStatus NameScanner(char Character, int Position)
        {
            // Variable name cannot start with digit. Otherwise it will be read as two parts.
            if ((char.IsLetterOrDigit(Character)) || (Character == '_'))
                return ScannerStatus.Continued;

            return ScannerStatus.PreviousFinished;
        }

        public static ScannerStatus CrLfScanner(char Character, int Position)
        {
            switch (Position)
            {
                case 0:
                    if (Character == '\r')
                        return ScannerStatus.Continued;
                    break;
                case 1:
                    if (Character == '\n')
                        return ScannerStatus.Finished;
                    break;
            }
            return ScannerStatus.Unmatch;
        }
    }

    public class Tokenizer
    {
        SourceCodeReader Reader;

        LexiconRules RulesContainer;
        public Tokenizer(string Code)
        {
            Reader = new SourceCodeReader(ref Code);
            RulesContainer = new LexiconRules();
            TokenizerRules.LoadLexicalRules(RulesContainer);
        }

        public Token? NextToken()
        {
            if ((char)Reader.Peek() == 0)
                return null;
            LexiconRules.KeyCollection Rules = RulesContainer.Keys;
            BitArray Matches = new BitArray(Rules.Count, true);

            StringBuilder TokenBuffer = new StringBuilder();
            char Character = '\0';
            int CandidatePos = 0;
            int RemainingCandidates = Rules.Count;


            do
            {
                CandidatePos = 0;
                Character = Reader.NextChar();

                foreach(TokenizerRules.LexiconType Candidate in Rules)
                {
                    if (Matches[CandidatePos])
                    {
                        switch (RulesContainer[Candidate].Scan(Character, TokenBuffer.Length))
                        {
                            case TokenizerRules.ScannerStatus.Continued:
                                break;
                            case TokenizerRules.ScannerStatus.Finished:
                                TokenBuffer.Append(Character);
                                return new Token
                                {
                                    Type = Candidate,
                                    Value = TokenBuffer.ToString()

                                };
                            case TokenizerRules.ScannerStatus.PreviousFinished:
                                if (TokenBuffer.Length > 0)
                                {
                                    Reader.MovePrev();
                                    return new Token
                                    {
                                        Type = Candidate,
                                        Value = TokenBuffer.ToString()
                                    };
                                }
                                else
                                {
                                    Matches.Set(CandidatePos, false);
                                    --RemainingCandidates;
                                }
                                break;
                            case TokenizerRules.ScannerStatus.Unmatch:
                                Matches.Set(CandidatePos, false);
                                --RemainingCandidates;
                                break;
                        }
                    }
                    ++CandidatePos;
                }

                //Judge if there's still some candidates are true.

                if (RemainingCandidates > 0)
                {
                    TokenBuffer.Append(Character);
                    continue;
                }else
                    throw new TokenizerRules.UnexpectedCharacterException(Reader.Position, Character);

            } while ((char)Character != 0);

            return null;
        }

    }

    public class SourceCodeReader
    {
        string BaseString;

        int Pointer;
        public int Position
        {
            get { return Pointer; }
        }

        public SourceCodeReader(ref string BaseString)
        {
            this.BaseString = BaseString;
        }

        [System.Diagnostics.DebuggerStepThrough()]
        public char Peek()
        {
            if (Pointer < 0 || Pointer >= BaseString.Length)
            {
                return (char)0;
            }
            else
            {
                return BaseString[Pointer];
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        public char NextChar()
        {
            char result = Peek();
            MoveNext();
            return result;
        }

        //Reserve
        public bool NextChar(char ch)
        {
            if (Peek() == ch)
            {
                MoveNext();
                return true;
            }
            else
            {
                return false;
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        public void MoveNext()
        {
            ++Pointer;
        }

        [System.Diagnostics.DebuggerStepThrough()]
        public void MovePrev()
        {
            --Pointer;
        }
    }
}