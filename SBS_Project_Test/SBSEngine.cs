using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBSEnvironment.Parsing;

namespace SBSEnvironment
{
    class SBSEngine
    {
        Parser parser;

        public void LoadSourceCode(string source)
        {
            parser = Parser.CreateParserFromString(source);
        }
    }
}
