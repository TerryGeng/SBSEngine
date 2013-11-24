using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBSEngine.Tokenization
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

    public class SBSRulesProvider : IRulesProvider
    {
        private struct ScannerResult
        {
            public ProviderResultCode result;
            public int status;
        }

        readonly static ScannerResult RESULT_UNDEF = new ScannerResult { result = ProviderResultCode.Undefined };
        readonly static ScannerResult RESULT_CONTINUED = new ScannerResult { result = ProviderResultCode.Continued };
        readonly static ScannerResult RESULT_PRE_FIN = new ScannerResult { result = ProviderResultCode.PreviousFinished };
        readonly static ScannerResult RESULT_FIN = new ScannerResult { result = ProviderResultCode.Finished };
        readonly static ScannerResult RESULT_UNMATCH = new ScannerResult { result = ProviderResultCode.Unmatch };


        Dictionary<int, int> statusPool;

        public SBSRulesProvider()
        {
            statusPool = new Dictionary<int, int>(7);
            statusPool.Add((int)LexiconType.LInteger, 0);
            statusPool.Add((int)LexiconType.LName, 0);
            statusPool.Add((int)LexiconType.LBlank, 0);
            statusPool.Add((int)LexiconType.LCrLf, 0);
        }

        int[] IRulesProvider.GetLexiconType()
        {
            return new int[] { (int)LexiconType.LInteger, (int)LexiconType.LName , (int)LexiconType.LBlank , (int)LexiconType.LCrLf };
        }

        ProviderResultCode IRulesProvider.CallScanner(int scanner, char character)
        {
            int status;
            ScannerResult result = RESULT_UNDEF;

            switch((LexiconType)scanner)
            {
                case LexiconType.LInteger :
                    result = IntegerScanner(character);
                    break;
                case LexiconType.LName :
                    result = NameScanner(character);
                    break;
                case LexiconType.LBlank :
                    result = BlankScanner(character);
                    break;
                case LexiconType.LCrLf :
                    status = statusPool[scanner];
                    result = CrLfScanner(character, status);
                    break;
            }

            statusPool[scanner] = result.status;

            return result.result;
        }

        void IRulesProvider.ResetScanner() 
        {
            statusPool[(int)LexiconType.LInteger] = 0;
            statusPool[(int)LexiconType.LName] = 0;
            statusPool[(int)LexiconType.LBlank] = 0;
            statusPool[(int)LexiconType.LCrLf] = 0;
        }

        // Following are scanners.

        private ScannerResult IntegerScanner(char character)
        {
            return char.IsDigit(character) ? RESULT_CONTINUED : RESULT_PRE_FIN;
        }

        private ScannerResult BlankScanner(char character)
        {
            // Character is neither vbCr nor vbLf
            return ((!char.IsControl(character)) && char.IsWhiteSpace(character)) ? RESULT_CONTINUED : RESULT_PRE_FIN;
        }

        private ScannerResult NameScanner(char character)
        {
            // Variable name cannot start with digit. Otherwise it will be read as two parts.
            return ((char.IsLetterOrDigit(character)) || (character == '_')) ? RESULT_CONTINUED : RESULT_PRE_FIN;
        }

        private ScannerResult CrLfScanner(char character, int status)
        {
            switch (status)
            {
                case 0:
                    if (character == '\r')
                        return new ScannerResult { result = ProviderResultCode.Continued , status = 1};
                    break;
                case 1:
                    if (character == '\n')
                        return RESULT_FIN;
                    break;
            }
            return RESULT_UNMATCH;
        }
    }
}
