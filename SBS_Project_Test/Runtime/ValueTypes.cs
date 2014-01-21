using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBSEngine.Runtime.Types
{
    public class IntegerValue : IConstValue
    {
        private int value;
        public void Accept(ExprCalcu visitor){}

        public IntegerValue(int value)
        {
            this.value = value;
        }

        public string ToString()
        {
            return value.ToString();
        }
    }

    public class DoubleValue : IConstValue
    {
        private double value;
        public void Accept(ExprCalcu visitor){}

        public DoubleValue(double value)
        {
            this.value = value;
        }

        public string ToString()
        {
            return value.ToString();
        }
    }
}
