using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A SmallDictionary is a lightweight dictionary, typically for ten
    /// entries or less, that uses less memory and is faster to construct
    /// that a traditional dictionary but with comparable performance.  It's
    /// performance is, however, very poor for a large number of entries.
    /// </summary>
    /// <typeparam name="TKey">The dictionary key</typeparam>
    /// <typeparam name="TValue">The dicttionary value</typeparam>
#if DEBUG
    [DebuggerDisplay("Parameters = {Count}"), DebuggerTypeProxy(typeof(DictionaryEntryDebugView))]
#endif
    public class SmallDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IEquatable<TKey>
    {
        private List<KeyValuePair<TKey, TValue>> pairs = new List<KeyValuePair<TKey, TValue>>();

        private int TryGet(TKey key)
        {
            for (int i = 0; i < pairs.Count; i++) if (pairs[i].Key.Equals(key)) return i;
            return -1;
        }

        private int Get(TKey key)
        {
            var index = TryGet(key);
            if (index == -1)
            {
                index = pairs.Count;
                pairs.Add(new KeyValuePair<TKey, TValue>(key, default(TValue)));
            }
            return index;
        }

        private void ValidateKey(TKey key)
        {
            if (!ContainsKey(key)) ThrowHelper.Throw("key not found: " + key);
        }

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key)) ThrowHelper.Throw("duplicate key: " + key);
            pairs.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            return TryGet(key) != -1;
        }

        public ICollection<TKey> Keys
        {
            get { return pairs.Select(pair => pair.Key).ToArray(); }
        }

        public bool Remove(TKey key)
        {
            var index = TryGet(key);
            if (index == -1) return false;
            pairs.RemoveAt(index);
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            var index = TryGet(key);
            if (index == -1) return false;
            value = pairs[index].Value;
            return true;
        }

        public ICollection<TValue> Values
        {
            get { return pairs.Select(pair => pair.Value).ToArray(); }
        }

        public TValue this[TKey key]
        {
            get { ValidateKey(key); return pairs[Get(key)].Value; }
            set { pairs[Get(key)] = new KeyValuePair<TKey, TValue>(key, value); }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            pairs.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!ContainsKey(item.Key)) return false;
            return pairs[Get(item.Key)].Value.Equals(item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            pairs.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return pairs.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!Contains(item)) return false;
            Remove(item.Key);
            return true;
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return pairs.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < pairs.Count; i++) yield return new DictionaryEntry(pairs[i].Key, pairs[i].Value);
        }

        #endregion
    }
}
