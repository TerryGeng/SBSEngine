using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SBSEngine.Runtime
{
    // TODO: This is a dirty work, should perfect it.
    class TypeConversion
    {
        public enum AbstractType
        {
            Undefined,

            Numeric,
            String,
            Array
        }

        struct TypeInfo
        {
            public AbstractType Abstract;
            public Type BaseType;
            public Type BranchTop;
            public int Weight;
        }

        /*
         *  TODO: Implicit Conversion Rules
         *  Only who has the same branch top can be convert implicitly.
         *  
         *                (B1)            (B2)
         *               Int(W1)         
         *                 |
         *               Double(W2)
         *                 |
         *  BranchTop:   String(W3)     Array(W1)(Not implemented)
         * 
         */

        static readonly TypeInfo TInteger = new TypeInfo
        {
            Abstract = AbstractType.Numeric,
            BaseType = typeof(int),
            BranchTop = typeof(String),
            Weight = 1
        };

        static readonly TypeInfo TDouble = new TypeInfo
        {
            Abstract = AbstractType.Numeric,
            BaseType = typeof(double),
            BranchTop = typeof(String),
            Weight = 2
        };

        static readonly TypeInfo TString = new TypeInfo
        {
            Abstract = AbstractType.String,
            BaseType = typeof(String),
            BranchTop = typeof(String),
            Weight = 3
        };

        static public AbstractType Implicit(ref Expression exprA,ref Expression exprB)
        {
            TypeInfo typeA = GetTypeInfo(exprA.Type);

            if (exprA.Type != exprB.Type)
            {
                TypeInfo typeB = GetTypeInfo(exprB.Type);

                if (typeA.BranchTop == typeB.BranchTop)
                {
                    if (typeA.Weight > typeB.Weight)
                    {
                        exprB = Expression.Convert(exprB,typeA.BaseType);
                        return typeA.Abstract;
                    }
                    else
                    {
                        exprA = Expression.Convert(exprA, typeB.BaseType);
                        return typeB.Abstract;
                    }
                        
                }

                throw new ApplicationException("Different base type."); // TODO: May be different base type can be convert.
            }
            else
            {
                return typeA.Abstract;
            }
        }

        static private TypeInfo GetTypeInfo(Type type) // TODO: May cause value copy.
        {
            if (type == typeof(int))
            {
                return TInteger;
            }
            else if (type == typeof(double))
            {
                return TDouble;
            }
            else if (type == typeof(String))
            {
                return TString;
            }
            else
            {
                throw new ApplicationException("Unknown type.");
            }
        }
    }
}
