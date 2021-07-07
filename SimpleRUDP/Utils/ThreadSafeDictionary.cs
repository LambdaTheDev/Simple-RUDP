using System.Collections.Generic;

namespace Simple_RUDP.Utils
{
    // Yeah, it's not complete, but I just need those.
    public class ThreadSafeDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _wrappedDictionary;

        public ThreadSafeDictionary()
        {
            _wrappedDictionary = new Dictionary<TKey, TValue>();
        }

        public ThreadSafeDictionary(int size)
        {
            _wrappedDictionary = new Dictionary<TKey, TValue>(size);
        }

        public int Count
        {
            get
            {
                lock (_wrappedDictionary)
                {
                    return _wrappedDictionary.Count;
                }
            }
        }

        public Dictionary<TKey, TValue>.ValueCollection Values
        {
            get
            {
                lock (_wrappedDictionary)
                {
                    return _wrappedDictionary.Values;
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (_wrappedDictionary)
            {
                _wrappedDictionary.Add(key, value);
            }
        }

        public void Remove(TKey key)
        {
            lock (_wrappedDictionary)
            {
                _wrappedDictionary.Remove(key);
            }
        }

        public void Clear()
        {
            lock (_wrappedDictionary)
            {
                _wrappedDictionary.Clear();
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (_wrappedDictionary)
            {
                return _wrappedDictionary.ContainsKey(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_wrappedDictionary)
            {
                return _wrappedDictionary.TryGetValue(key, out value);
            }
        }
        
        public TValue this[TKey key]
        {
            get
            {
                lock (_wrappedDictionary)
                {
                    return _wrappedDictionary[key];
                }
            }
            set
            {
                lock (_wrappedDictionary)
                {
                    _wrappedDictionary[key] = value;
                }
            }
        }
    }
}