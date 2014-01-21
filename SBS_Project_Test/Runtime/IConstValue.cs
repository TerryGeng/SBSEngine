using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBSEngine.Runtime.Types
{
    public interface IConstValue
    {
        void Accept(ExprCalcu visitor);
        string ToString();
    }
}
