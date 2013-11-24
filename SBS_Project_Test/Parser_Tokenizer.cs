using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using LexiconRules = System.Collections.Generic.Dictionary<SBSEngine.Tokenization.LexiconType, SBSEngine.Tokenization.ScannerFunction>;

namespace SBSEngine.Tokenization // Tokenizer configurations part
{
    public enum LexiconType
    {
        Undefined,

        // Set lexicon type below.
        LInteger,
        LFloat,
        LName,
        LString,
        LBlank,
        LCrLf,
        LSymbol
    }

    static public class TokenizerRules
    {
        public static void LoadLexicalRules(LexiconRules container)
        {
            // Add available types and corresponding scanners.
            container.Add(LexiconType.LInteger, IntegerScanner);
            container.Add(LexiconType.LBlank, BlankScanner);
            container.Add(LexiconType.LName, NameScanner);
            container.Add(LexiconType.LCrLf, CrLfScanner);
        }

        // Add your scanner below.

        public static ScannerStatus IntegerScanner(char character, int position)
        {
            return char.IsDigit(character) ? ScannerStatus.Continued : ScannerStatus.PreviousFinished;
        }

        public static ScannerStatus BlankScanner(char character, int position)
        {
            // Character is neither vbCr nor vbLf
            return ((!char.IsControl(character)) && char.IsWhiteSpace(character)) ? ScannerStatus.Continued : ScannerStatus.PreviousFinished;
        }

        public static ScannerStatus NameScanner(char character, int position)
        {
            // Variable name cannot start with digit. Otherwise it will be read as two parts.
            return ((char.IsLetterOrDigit(character)) || (character == '_')) ? ScannerStatus.Continued : ScannerStatus.Continued;
        }

        public static ScannerStatus CrLfScanner(char character, int position)
        {
            switch (position)
            {
                case 0:
                    if (character == '\r')
                        return ScannerStatus.Continued;
                    break;
                case 1:
                    if (character == '\n')
                        return ScannerStatus.Finished;
                    break;
            }
            return ScannerStatus.Unmatch;
        }
    }
}

namespace SBSEngine.Tokenization // Tokenizer core part
{
    public struct Token
    {
        public LexiconType Type;
        public string Value;
    }

    public delegate ScannerStatus ScannerFunction(char character, int position);

    public enum ScannerStatus
    {
        /// <summary>
        /// Current and previous characters still match the rule. Match process continue.
        /// </summary>
        Continued,
        /// <summary>
        /// The emergence of current char indicated this combination unmatched the rule.
        /// </summary>
        Unmatch,
        /// <summary>
        /// Current and previous characters matchs the rule. Match process end.
        /// </summary>
        Finished,
        /// <summary>
        /// Only previous characters match the rule. Match process end.
        /// </summary>
        PreviousFinished
    }

    public class UnexpectedCharacterException : ApplicationException
    {
        public int Position;

        public char Character;
        public UnexpectedCharacterException(int position, char character)
            : base(string.Format("Unexpected Character '{0}'({1}) at {2}.", character, (int)character, position))
        {
            this.Position = position;
            this.Character = character;
        }
    }


    public class Tokenizer
    {
        SourceCodeReader reader;
        LexiconRules rulesContainer;

        public Tokenizer(string code)
        {
            reader = new SourceCodeReader(code);
            rulesContainer = new LexiconRules();
            TokenizerRules.LoadLexicalRules(rulesContainer);
        }

        public Token? NextToken()
        {
            if (reader.Peek() == 0)
                return null;
            LexiconRules.KeyCollection rules = rulesContainer.Keys;
            BitArray matches = new BitArray(rules.Count, true);

            StringBuilder tokenBuffer = new StringBuilder();
            char character = '\0';
            int candidatePos;
            int remaining = rules.Count;

            do
            {
                candidatePos = 0;
                character = reader.NextChar();

                foreach (LexiconType candidate in rules)
                {
                    if (matches[candidatePos])
                    {
                        switch (rulesContainer[candidate](character, tokenBuffer.Length))
                        {
                            case ScannerStatus.Continued:
                                break;
                            case ScannerStatus.Finished:
                                tokenBuffer.Append(character);
                                return new Token
                                {
                                    Type = candidate,
                                    Value = tokenBuffer.ToString()
                                };
                            case ScannerStatus.PreviousFinished:
                                if (tokenBuffer.Length > 0)
                                {
                                    reader.MovePrev();
                                    return new Token
                                    {
                                        Type = candidate,
                                        Value = tokenBuffer.ToString()
                                    };
                                }
                                else
                                {
                                    matches.Set(candidatePos, false);
                                    --remaining;
                                }
                                break;
                            case ScannerStatus.Unmatch:
                                matches.Set(candidatePos, false);
                                --remaining;
                                break;
                        }
                    }
                    ++candidatePos;
                }

                // Check if there's still some candidates true
                if (remaining > 0)
                {
                    tokenBuffer.Append(character);
                    continue;
                }
                else throw new UnexpectedCharacterException(reader.Position, character);
            } while (character != 0);

            return null;
        }
    }

    public class SourceCodeReader
    {
        string baseString;

        int pointer;
        public int Position
        {
            get { return pointer; }
        }

        public SourceCodeReader(string BaseString)
        {
            this.baseString = BaseString;
        }

        [DebuggerStepThrough]
        public char Peek()
        {
            if (pointer < 0 || pointer >= baseString.Length)
                return (char)0;
            else return baseString[pointer];
        }

        [DebuggerStepThrough]
        public char NextChar()
        {
            char result = Peek();
            MoveNext();
            return result;
        }

        // Reserve
        public bool NextChar(char ch)
        {
            if (Peek() == ch)
            {
                MoveNext();
                return true;
            }
            else return false;
        }

        [DebuggerStepThrough]
        public void MoveNext() { ++pointer; }

        [DebuggerStepThrough]
        public void MovePrev() { --pointer; }
    }
}