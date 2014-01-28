using System;
using System.IO;
using System.Collections.Generic;
using MSAst = System.Linq.Expressions;
using SBSEngine.Tokenization;
using System.Text;

namespace SBSEngine.Parsing 
{
    class Parser
    {
        private SourceContent content;

        public static Parser CreateParserFromFile(string fileName,Encoding encoding){
            Parser p = new Parser();

            p.content = new SourceContent(new StreamReader(new FileStream(fileName,FileMode.Open),encoding));

            return p;
        }

        public static Parser CreateParserFromString(string code)
        {
            Parser p = new Parser();

            p.content = new SourceContent(new StringReader(code));

            return p;
        }

        public MSAst.Expression Parse()
        {

        }
    }
}
