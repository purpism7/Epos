using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Common
{
    // <summary>
    /// 키 하나에 여러 타입의 값을 붙일 수 있는 약참조 맵.
    /// 동일 키에서 TValue 타입마다 1개씩 저장됩니다.
    /// - 키는 <see cref="ConditionalWeakTable{TKey, TValue}"/> 로 관리되어,
    ///   키가 GC로 수거되면 엔트리가 자동 제거되어 누수를 줄여줍니다.
    /// - 스레드 안전: 키별 Bag 객체에 lock을 걸어 보호합니다.
    /// </summary>
    public sealed class WeakTypeMap<TKey> where TKey : class
    {
        private readonly ConditionalWeakTable<TKey, Bag> _table = new();

        // 키별로 타입→값을 보관하는 작은 컨테이너
        private sealed class Bag
        {
            public readonly Dictionary<Type, object> Values = new();
        }

        /// <summary>
        /// 지정 키에 값 타입(TValue) 슬롯으로 값을 설정합니다.
        /// 동일 타입 슬롯이 이미 있으면 덮어씁니다.
        /// </summary>
        public void Set<TValue>(TKey key, TValue value) where TValue : class
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            var bag = _table.GetOrCreateValue(key);
            lock (bag)
            {
                bag.Values[typeof(TValue)] = value;
            }
        }

        /// <summary>
        /// 지정 키에서 값 타입(TValue) 슬롯을 조회합니다.
        /// </summary>
        public bool TryGet<TValue>(TKey key, out TValue value) where TValue : class
        {
            value = null!;
            if (key is null) return false;

            if (_table.TryGetValue(key, out var bag))
            {
                lock (bag)
                {
                    if (bag.Values.TryGetValue(typeof(TValue), out var obj) && obj is TValue t)
                    {
                        value = t;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 존재하면 반환, 없으면 factory로 생성하여 저장 후 반환합니다.
        /// </summary>
        public TValue GetOrAdd<TValue>(TKey key, Func<TKey, TValue> factory) where TValue : class
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (factory is null) throw new ArgumentNullException(nameof(factory));

            var bag = _table.GetOrCreateValue(key);
            lock (bag)
            {
                if (bag.Values.TryGetValue(typeof(TValue), out var obj) && obj is TValue existed)
                    return existed;

                var created = factory(key) ?? throw new InvalidOperationException("Factory returned null.");
                bag.Values[typeof(TValue)] = created;

                return created;
            }
        }

        /// <summary>
        /// 지정 키에서 값 타입(TValue) 슬롯을 제거합니다.
        /// </summary>
        public bool Remove<TValue>(TKey key) where TValue : class
        {
            if (key is null) return false;

            if (_table.TryGetValue(key, out var bag))
            {
                lock (bag)
                {
                    return bag.Values.Remove(typeof(TValue));
                }
            }

            return false;
        }

        /// <summary>
        /// 지정 키와 연결된 모든 타입 슬롯을 제거합니다.
        /// (키 자체의 Bag을 테이블에서 제거)
        /// </summary>
        public bool RemoveAll(TKey key)
        {
            if (key is null) 
                return false;

            return _table.Remove(key);
        }

        /// <summary>
        /// Set과 동일하되, 이미 있으면 덮어쓰고 결과를 반환합니다.
        /// </summary>
        public TValue AddOrUpdate<TValue>(TKey key, TValue value) where TValue : class
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            var bag = _table.GetOrCreateValue(key);
            lock (bag)
            {
                bag.Values[typeof(TValue)] = value;

                return value;
            }
        }
    }
}
