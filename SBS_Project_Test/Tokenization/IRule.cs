using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBSEnvironment.Tokenization
{
    public interface IRule
    {
        ScannerStatus Scan(int character);
        Token Pack(StringBuilder buffer);
        void Reset();
    }
}
