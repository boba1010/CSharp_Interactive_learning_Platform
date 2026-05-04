using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp_Interpreter.Utils
{
    public static class DictionaryExtension
    {
        public static bool Update<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict !=  null && dict.ContainsKey(key))
            {
                dict[key] = value;
                return true;
            }
            return false;
        }
    }
}
