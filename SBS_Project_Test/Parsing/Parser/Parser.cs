using System;
using System.IO;
using System.Collections.Generic;
using MSAst = System.Linq.Expressions;
using SBSEngine.Tokenization;
using System.Text;
using SBSEngine.Parsing.Packer;

namespace SBSEngine.Parsing 
{
    public class Parser
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
            return ScopePacker.Pack(context).Reduce();
        }
    }
}
