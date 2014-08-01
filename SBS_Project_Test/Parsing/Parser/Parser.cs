using System;
using System.IO;
using System.Collections.Generic;
using MSAst = System.Linq.Expressions;
using SBSEnvironment.Tokenization;
using SBSEnvironment.Runtime;
using System.Text;

namespace SBSEnvironment.Parsing 
{
    public partial class Parser
    {
        private ParsingContext context;
        private ExecutableUnit unit;

        public static Parser CreateParserFromFile(string fileName, Encoding encoding){
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
            return MSAst.Expression.Convert(PackScope().Reduce(), typeof(object));
        }
    }
}
