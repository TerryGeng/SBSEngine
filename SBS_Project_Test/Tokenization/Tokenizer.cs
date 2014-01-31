using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SBSEngine.Tokenization // Tokenizer core part
{
    public struct Token
    {
        public int Type;
        public string Value;
    }

    public struct ScannerResult
    {
        public ScannerStatus Result;
        public ReadingOption Option;
    }

    public enum ReadingOption
    {
        Normal,

        /// <summary>
        /// Discard current token, and do ReadToken() again. 
        /// </summary>
        IgnoreCurrent,

        /// <summary>
        /// Interrupt the reading process, for ReadToken() and scanner while meeting eof.
        /// </summary>
        FinishReading
    }

    public enum ScannerStatus
    {
        Undefined,

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
        TextReader reader;
        IList<IRule> rules;
        BitArray matches; // Available rules for ReadToken().
        ReadingOption lastReadingOption; 
        StringBuilder tokenBuffer;
        Token lastPeek; // If this is not empty, NextToken() will directly return this and empty this with a null token;

        int currentPosition = 1;
        int virtualPosition = 1;

        public int Position
        {
            get
            {
                if (lastPeek.Type != 0)
                    return virtualPosition;
                return currentPosition;
            }
        }

        public Tokenizer(IList<IRule> rules, TextReader codeReader)
        {
            reader = codeReader;
            this.rules = rules;
            matches = new BitArray(rules.Count, true);
            tokenBuffer = new StringBuilder();
        }

        public Token PeekToken()
        {
            if (lastPeek.Type == 0)
            {
                virtualPosition = currentPosition;
                lastPeek = NextToken();
            }
            return lastPeek;
        }

        public int PeekTokenType()
        {
            return PeekToken().Type;
        }

        public int NextTokenType()
        {
            return NextToken().Type;
        }

        public bool NextTokenType(int type)
        {
            if (PeekTokenType() == type)
            {
                NextToken();
                return true;
            }

            return false;
        }

        public Token NextToken()
        {
            if (lastPeek.Type != 0)
            {
                Token token = lastPeek;
                lastPeek = new Token();
                return token;
            }

            int matched;

            while (true)
            {
                matched = ReadToken();
                switch (lastReadingOption)
                {
                    case ReadingOption.FinishReading: 
                        return new Token();
                    case ReadingOption.IgnoreCurrent:
                        continue;
                    case ReadingOption.Normal:
                    default:
                        return rules[matched].Pack(tokenBuffer);
                }
            }

        }

        // return val: Matched rules.
        private int ReadToken()
        {
            if (reader.Peek() == -1)
            {
                lastReadingOption = ReadingOption.FinishReading;
                return -1;
            }

            
            int character;
            int remaining = rules.Count;

            this.ResetFormerStatus();

            do
            {
                character = reader.Peek();

                for (int i = 0; i < rules.Count; ++i)
                {
                    if (!matches[i]) continue;
                    ScannerResult result = rules[i].Scan(character);

                    switch (result.Result)
                    {
                        case ScannerStatus.Continued:
                            break;
                        case ScannerStatus.Finished:
                            tokenBuffer.Append(character);
                            reader.Read();
                            currentPosition += 1;
                            lastReadingOption = result.Option;
                            return i;
                        case ScannerStatus.PreviousFinished:
                            if (tokenBuffer.Length > 0)
                            {
                                lastReadingOption = result.Option;
                                return i;
                            }
                            else
                            {
                                matches.Set(i, false);
                                --remaining;
                            }
                            break;
                        case ScannerStatus.Unmatch:
                            matches.Set(i, false);
                            --remaining;
                            break;
                    }
                }

                // Check if there's still some candidates true
                if (remaining > 0)
                {
                    tokenBuffer.Append((char)character);
                    reader.Read();
                    currentPosition += 1;
                    continue;
                }
                else throw new UnexpectedCharacterException(currentPosition, (char)character);
            } while (character != -1);

            return -1;
        }

        private void ResetFormerStatus()
        {
            for (int i = 0; i < rules.Count; ++i)
            {
                rules[i].Reset();
            }

            matches.SetAll(true);
            tokenBuffer.Clear();
        }
    }
}
