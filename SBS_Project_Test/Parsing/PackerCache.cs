using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBSEngine.Tokenization;

namespace SBSEngine.Parsing
{
    class PackerCache<T> where T:IPacker,new()
    {
        static T instance;

        static public T Instance(Tokenizer tokenizer,Scope scope)
        {
            if (instance == null)
            {
                instance = new T();
            }

            instance.Tokenizer = tokenizer;
            instance.Scope = scope;

            return instance;
        }
    }
}
