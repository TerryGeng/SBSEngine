using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SBSEngine.Tokenization;
using System.Text;

namespace SBSEngine.Parsing
{
    class AssignExpressionPacker : Packer
    {
        public Tokenizer Tokenizer { private get; set; }
        public Scope Scope { private get; set; }

        private Stack<String> leftName;
        private Expression right;
        private Expression mainExpr;

        public AssignExpressionPacker()
        {
            leftName = new Stack<String>(10);
        }

        /*
         * Assignment = (Variable  ('='|<TODO>))*  Expression
         *                     (*Left*)             (*Right*）
         */
        public Expression PackStatment()
        {
            Token nameToken;

            // (*Left*)
            while(true){
                // Variable = '$' Name
                if (!Tokenizer.TestNextTokenType((int)LexiconType.LSDollar)){
                    if (leftName.Count != 0)
                        break;
                    else
                        ParsingError.ThrowUnexpectedTokenException("Invalid left side of assign expression.");
                    }
                
                if((nameToken = Tokenizer.PeekToken()).Type == (int)LexiconType.LName)
                   Tokenizer.NextToken();
                else
                    ParsingError.ThrowUnexpectedTokenException("Invalid left side of assign expression.");

                leftName.Push(nameToken.Value);

                // '='
                if (!Tokenizer.TestNextTokenType((int)LexiconType.LSEqual))
                {
                    if (leftName.Count > 1)
                        break;
                    else
                        ParsingError.ThrowUnexpectedTokenException("Invalid assignment operator.");
                }
            }

            // (*Right*)
            right = PackerCache<ExpressionPacker>.Instance(Tokenizer,Scope).PackStatment();

            return CombineLeftAndRight();
        }

        private Expression CombineLeftAndRight()
        {
            foreach (string name in leftName)
            {
                mainExpr = Expression.Assign(Scope.GetVariableExpr(name,right.Type),right);
            }

            return mainExpr;
        }
    }
}
