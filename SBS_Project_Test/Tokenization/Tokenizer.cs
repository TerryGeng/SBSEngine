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

    public enum ReadingOption
    {
        Normal,

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
        StringBuilder tokenBuffer;

        int currentPosition = 1;

        public int Position
        {
            get
            {
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

        public Token NextToken()
        {
            if (reader.Peek() == -1)
            {
                return new Token();
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
                    ScannerStatus result = rules[i].Scan(character);

                    switch (result)
                    {
                        case ScannerStatus.Continued:
                            break;
                        case ScannerStatus.Finished:
                            tokenBuffer.Append(character);
                            reader.Read();
                            currentPosition += 1;
                            return rules[i].Pack(tokenBuffer);
                        case ScannerStatus.PreviousFinished:
                            if (tokenBuffer.Length > 0)
                            {
                                return rules[i].Pack(tokenBuffer);
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

            return new Token();
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
