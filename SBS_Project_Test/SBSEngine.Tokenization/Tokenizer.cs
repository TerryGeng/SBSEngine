using System;
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

    public enum ScannerResult
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
        SourceCodeReader reader;
        List<IRule> rules;

        public Tokenizer(List<IRule> rules, string code)
        {
            reader = new SourceCodeReader(code);
            this.rules = rules;
        }

        public Token? NextToken()
        {
            if (reader.Peek() == 0)
                return null;
            BitArray matches = new BitArray(rules.Count, true);

            StringBuilder tokenBuffer = new StringBuilder();
            char character = '\0';
            int candidatePos;
            int remaining = rules.Count;

            this.resetRules();

            do
            {
                candidatePos = 0;
                character = reader.NextChar();

                for (int i = 0; i < rules.Count && matches[i]; ++i)
                {
                    switch (rules[i].scan(character))
                    {
                        case ScannerResult.Continued:
                            break;
                        case ScannerResult.Finished:
                            tokenBuffer.Append(character);
                            return rules[i].pack(tokenBuffer);
                        case ScannerResult.PreviousFinished:
                            if (tokenBuffer.Length > 0)
                            {
                                reader.MovePrev();
                                return rules[i].pack(tokenBuffer);
                            }
                            else
                            {
                                matches.Set(candidatePos, false);
                                --remaining;
                            }
                            break;
                        case ScannerResult.Unmatch:
                            matches.Set(candidatePos, false);
                            --remaining;
                            break;
                    }
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

        private void resetRules()
        {
            for (int i = 0; i < rules.Count; ++i)
            {
                rules[i].reset();
            }
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