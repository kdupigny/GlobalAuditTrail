using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GATUtils.Types.Custom
{
    public static class DictionaryExtensions
    {
        public static TKey FindKeyByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
                if (value.Equals(pair.Value)) return pair.Key;

            throw new Exception("the value is not found in the dictionary");
        }

        public static int FindIndexByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            int index = 0;
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                if (value.Equals(pair.Value)) return index;
                index++;
            }

            throw new Exception("the value is not found in the dictionary");
        }
    }
}
