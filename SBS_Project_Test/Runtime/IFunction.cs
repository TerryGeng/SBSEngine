using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBSEnvironment.Runtime
{
    interface IFunction
    {
        string Name { get; }
        int ArgCount { get; }
        object Emit(object[] args);
    }
}
