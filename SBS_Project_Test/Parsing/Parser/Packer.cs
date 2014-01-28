using System.Linq.Expressions;
using SBSEngine.Tokenization;

namespace SBSEngine.Parsing
{
    /* Note:
     * A Packer is just a collection of method to parse certain types of Statment.
     * Though is this a normal class, but it should only have static methods.
     * Its methods will be called without creating an instance. So do not add any 
     * non-static things or static fields which may store current status of parsing to a Packer.
     */
    abstract class Packer
    {
        public static virtual Expression PackStatment(SourceContent content){
            return null;
        }
    }
}
