using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBSEngine.Tokenization
{
    public interface IRule
    {
        ScannerResult scan(int character);
        Token pack(StringBuilder buffer);
        void reset();
    }
}
