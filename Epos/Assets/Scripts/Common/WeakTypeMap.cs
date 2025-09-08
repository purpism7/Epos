using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Common
{
    public sealed class WeakTypeMap<TKey> where TKey : class
    {
        private readonly ConditionalWeakTable<TKey, Bag> _table = new();

        private sealed class Bag
        {
            public readonly Dictionary<Type, object> Values = new();
        }

        public void Set<TValue>(TKey key, TValue value) where TValue : class
        {
            var bag = _table.GetOrCreateValue(key);
            lock (bag.Values) bag.Values[typeof(TValue)] = value!;
        }

        public bool TryGet<TValue>(TKey key, out TValue value) where TValue : class
        {
            if (_table.TryGetValue(key, out var bag))
            {
                lock (bag.Values)
                {
                    if (bag.Values.TryGetValue(typeof(TValue), out var obj) && obj is TValue t)
                    { value = t; return true; }
                }
            }
            value = null!;

            return false;
        }

        public bool Remove<TValue>(TKey key) where TValue : class
        {
            return _table.TryGetValue(key, out var bag) && bag.Values.Remove(typeof(TValue));
        }

        public bool RemoveAll(TKey key) => _table.Remove(key);
    }
}
