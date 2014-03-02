﻿using System;
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

        public override System.Linq.Expressions.Expression Bind(object[] args, System.Collections.ObjectModel.ReadOnlyCollection<System.Linq.Expressions.ParameterExpression> parameters, System.Linq.Expressions.LabelTarget returnLabel)
        {
            return null;
        }

        public override T BindDelegate<T>(CallSite<T> site, object[] args)
        {
            Type t1 = args[0].GetType();
            Type t2 = args[1].GetType();

            switch ((SBSOperator)args[2])
            {
                case SBSOperator.Add:
                    return BinaryOperationDelegate.AddOperation<T>(args[0], args[1]);
                case SBSOperator.Subtract:
                    return BinaryOperationDelegate.SubtractOperation<T>(args[0], args[1]);
                case SBSOperator.Multiply:
                    return BinaryOperationDelegate.MultiplyOperation<T>(args[0], args[1]);
                case SBSOperator.Divide:
                    return BinaryOperationDelegate.DivideOperation<T>(args[0], args[1]);
                case SBSOperator.GreaterThan:
                    return BinaryOperationDelegate.GreaterThanOperation<T>(args[0], args[1]);
            }

            Debug.Assert(false,"Unimplemented op.");
            return null;
        }


    }
}
