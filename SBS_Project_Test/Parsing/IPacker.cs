using System.Linq.Expressions;
using SBSEngine.Tokenization;

namespace SBSEngine.Parsing
{
    interface IPacker
    {
        Tokenizer Tokenizer { set; }

        Scope Scope { set; }

        Expression PackStatment();
    }
}
