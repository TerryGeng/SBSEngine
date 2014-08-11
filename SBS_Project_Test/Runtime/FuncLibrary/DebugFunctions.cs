using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SBSEnvironment.Runtime.FuncLibrary
{
    class DebugFunctions
    {
        static public void LoadFunctions(ExecutableUnit unit)
        {
            unit.AddFunction(new LibFunction("DebugWriteLine", 1, DebugWriteLine));
        }

        static public object DebugWriteLine(object[] args)
        {
            Debug.WriteLine(args[0]);

            return null;
        }
    }
}
