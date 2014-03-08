using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using SBSEngine.Runtime;
using SBSEngine.Runtime.Binding;
using System.Diagnostics;
using SBSEngine.Runtime.Binding.Sorter;

namespace SBSEngine.Runtime.Binding
{
    /// <summary>
    /// This is a binder which provide runtime operations like add, subtract.
    /// </summary>
    class BinaryOpBinder : CallSiteBinder
    {
        private BinaryOpSorter sorter;

        public BinaryOpBinder(BinaryOpSorter sorter)
        {
            this.sorter = sorter;
        }

        public override System.Linq.Expressions.Expression Bind(object[] args, System.Collections.ObjectModel.ReadOnlyCollection<System.Linq.Expressions.ParameterExpression> parameters, System.Linq.Expressions.LabelTarget returnLabel)
        {
            return null;
        }

        public override T BindDelegate<T>(CallSite<T> site, object[] args)
        {
            Type t1 = args[0].GetType();
            Type t2 = args[1].GetType();
            SBSOperator op = (SBSOperator)args[2];

            return sorter.GetBinaryDelegate<T>(t1, t2, op);
        }


    }
}
