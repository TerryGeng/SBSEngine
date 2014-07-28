using System;
using System.IO;
using System.Collections.Generic;
using MSAst = System.Linq.Expressions;
using SBSEngine.Tokenization;
using System.Text;

namespace SBSEngine.Parsing 
{
    public partial class Parser
    {
        private ParsingContext context;

        public static Parser CreateParserFromFile(string fileName,Encoding encoding){
            Parser p = new Parser();

            p.context = new ParsingContext(new StreamReader(new FileStream(fileName, FileMode.Open), encoding));

            return p;
        }

        public static Parser CreateParserFromString(string code)
        {
            Parser p = new Parser();

            p.context = new ParsingContext(new StringReader(code));

            return p;
        }

        public MSAst.Expression Parse()
        {
            return PackScope().Reduce();
        }
    }
}
