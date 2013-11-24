using System;
using System.Collections.Generic;
using System.Text;

namespace SBSEngine.Tokenization
{
    public interface IRulesProvider
    {
        ProviderResultCode CallScanner(int scanner,char character);
        void ResetScanner();
        int[] GetLexiconType();
    }
}
