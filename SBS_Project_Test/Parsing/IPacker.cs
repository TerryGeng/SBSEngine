using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBSEngine.Tokenization;

namespace SBSEngine.Parsing
{
    interface IPacker
    {
        Tokenizer Tokenizer
        {
            set;
        }

        IStatment PackStatment();
    }
}
