using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBSEngine.Parsing
{
    public interface IStatment
    {
        void Accept(); // Visitor pattern
    }
}
