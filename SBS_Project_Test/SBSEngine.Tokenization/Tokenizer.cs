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

    public delegate ProviderResultCode ScannerFunction(char character, int position);

    public enum ProviderResultCode
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
        IRulesProvider rulesProvider;
        readonly int[] lexiconTypes;

        public Tokenizer(IRulesProvider rulesProvider,string code)
        {
            reader = new SourceCodeReader(code);
            this.rulesProvider = rulesProvider;
            lexiconTypes = this.rulesProvider.GetLexiconType();
        }

        public Token? NextToken()
        {
            if (reader.Peek() == 0)
                return null;
            BitArray matches = new BitArray(lexiconTypes.Length, true);

            StringBuilder tokenBuffer = new StringBuilder();
            char character = '\0';
            int candidatePos;
            int remaining = lexiconTypes.Length;

            rulesProvider.ResetScanner();

            do
            {
                candidatePos = 0;
                character = reader.NextChar();

                foreach (int candidate in lexiconTypes)
                {
                    if (matches[candidatePos])
                    {
                        switch (rulesProvider.CallScanner(candidate,character))
                        {
                            case ProviderResultCode.Continued:
                                break;
                            case ProviderResultCode.Finished:
                                tokenBuffer.Append(character);
                                return new Token
                                {
                                    Type = candidate,
                                    Value = tokenBuffer.ToString()
                                };
                            case ProviderResultCode.PreviousFinished:
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
                            case ProviderResultCode.Unmatch:
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