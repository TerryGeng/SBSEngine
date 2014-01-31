using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using SBSEngine.Runtime;

namespace SBSEngine.Runtime
{
    /// <summary>
    /// This is a binder which provide runtime operations like add, subtract.
    /// </summary>
    class BinaryOpBinder : CallSiteBinder
    {
        public override T BindDelegate<T>(CallSite<T> site, object[] args)
        {
            Type t1 = args[0].GetType();
            Type t2 = args[1].GetType();

            if (t1 == t2)
            {
                if (t1 == typeof(int))
                {
                    switch ((SBSOperator)args[3])
                    {
                        case SBSOperator.Add:
                            return AddOperation<T>(args[0],args[1]);
                    }
                    
                }
            }

            throw new ApplicationException();
        }

        public T AddOperation<T>(object left, object right)
        {
            if (left.GetType() == typeof(int))
            {
                return (T)(Object)new Func<CallSite, object, object, object>(IntAdd);
            }

            return default(T);
        }

        public object IntAdd(CallSite site, object left, object right)
        {
            if (left.GetType() == typeof(int) && right.GetType() == typeof(int))
            {
                return (int)left + (int)right;
            }

            return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, left, right);
        }
    }
}
