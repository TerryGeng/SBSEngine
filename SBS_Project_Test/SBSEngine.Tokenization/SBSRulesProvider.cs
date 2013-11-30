using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBSEngine.Tokenization
{
    
    public class SBSRulesProvider : IRulesProvider
    {
        public enum LexiconType
        {
            Undefined,

            // Set lexicon type below.
            LInteger,
            LFloat,
            LName,
            LBlank,
            LCrLf,
            LString,
            LSymbol
        }

        private struct ScannerResult
        {
            public ProviderResultCode result;
            public int status;
        }


        int[] statusPool;

        public SBSRulesProvider()
        {
            statusPool = new int[6];
        }

        int[] IRulesProvider.GetLexiconType()
        {
            return new int[] { (int)LexiconType.LInteger, (int)LexiconType.LFloat ,(int)LexiconType.LName , (int)LexiconType.LBlank , (int)LexiconType.LCrLf };
        }

        ProviderResultCode IRulesProvider.CallScanner(int scanner, char character)
        {
            int status = 0;
            ProviderResultCode result = ProviderResultCode.Undefined;

            switch((LexiconType)scanner)
            {
                case LexiconType.LInteger :
                    result = IntegerScanner(character);
                    break;
                case LexiconType.LFloat:
                    status = statusPool[scanner];
                    result = FloatScanner(character,ref status);
                    break;
                case LexiconType.LName :
                    status = statusPool[scanner];
                    result = NameScanner(character, ref status);
                    break;
                case LexiconType.LBlank :
                    result = BlankScanner(character);
                    break;
                case LexiconType.LCrLf :
                    status = statusPool[scanner];
                    result = CrLfScanner(character, ref status);
                    break;
            }

            statusPool[scanner] = status;

            return result;
        }

        void IRulesProvider.ResetScanner() 
        {
            Array.Clear(statusPool,0,statusPool.Length);
        }

        // Following are scanners.

        private ProviderResultCode IntegerScanner(char character)
        {
            return char.IsDigit(character) ? ProviderResultCode.Continued : ((character!='.') ? ProviderResultCode.PreviousFinished : ProviderResultCode.Unmatch);
        }

        private ProviderResultCode FloatScanner(char character, ref int status)
        {
            if (char.IsDigit(character))
            {
                if (status == 1) 
                    status = 2;
                return ProviderResultCode.Continued;
            }
            else if (character == '.' && status == 0)
            {
                status = 1;
                return ProviderResultCode.Continued;
            }

            return (status == 2) ? ProviderResultCode.PreviousFinished : ProviderResultCode.Unmatch;
        }

        private ProviderResultCode BlankScanner(char character)
        {
            // Character is neither vbCr nor vbLf
            return ((!char.IsControl(character)) && char.IsWhiteSpace(character)) ? ProviderResultCode.Continued : ProviderResultCode.PreviousFinished;
        }

        private ProviderResultCode NameScanner(char character, ref int status)
        {
            if (status == 0 && char.IsDigit(character))
            {
                return ProviderResultCode.Unmatch;
            }

            status = 1;

            if (char.IsLetterOrDigit(character) || character == '_')
                return ProviderResultCode.Continued;

            return ProviderResultCode.PreviousFinished;
        }

        private ProviderResultCode CrLfScanner(char character, ref int status)
        {
            switch (status)
            {
                case 0:
                    if (character == '\r')
                    {
                        status = 1;
                        return ProviderResultCode.Continued;
                    }
                    break;
                case 1:
                    if (character == '\n')
                        return ProviderResultCode.Finished;
                    break;
            }
            return ProviderResultCode.Unmatch;
        }
    }
}
