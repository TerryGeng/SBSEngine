using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBSEngine.Tokenization
{
    public interface IRule
    {
        ScannerResult Scan(int character);
        Token Pack(StringBuilder buffer);
        void Reset();
    }
}
