using System;
using System.Collections.Generic;
using System.Threading;

namespace Simply.Property
{
    /// <summary>
    /// Кэш объектов для работы в многопоточном режиме
    /// </summary>
    /// <typeparam name="Key">Тип ключа</typeparam>
    /// <typeparam name="Value">Тип значения</typeparam>
    public class SynchronizedCache<Key, Value> where Value : class
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private Dictionary<Key, Value> innerCache = new Dictionary<Key, Value>();
        /// <summary>
        /// Количество объктов в кэше
        /// </summary>
        public int Count => innerCache.Count;
        /// <summary>
        /// Возвращяет объект из кэша
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Объект из кэша</returns>
        public Value Get(Key key)
        {
            cacheLock.EnterReadLock();
            try
            {
                if (innerCache.ContainsKey(key))
                {
                    return innerCache[key];
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }
        /// <summary>
        /// Возвращяет объект из кэша или создает новый
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="сreateValue">Функция для создания объекта, вызывается в случае отсутствия объекта в кэше</param>
        public Value GetOrAdd(Key key, Func<Value> сreateValue)
        {
            cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (innerCache.ContainsKey(key) != true)
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        innerCache.Add(key, сreateValue());
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                }
                return innerCache[key];
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
        }
        /// <summary>
        /// Возвращяет объект из кэша или создает новый
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="сreateValue">Функция для создания объекта, вызывается в случае отсутствия объекта в кэше</param>
        /// <returns>Объект из кэша</returns>
        public Value GetOrCreate(Key key, Func<Value> сreateValue)
        {
            return Get(key) ?? GetOrAdd(key, сreateValue);
        }
        /// <summary>
        /// Дискриптор класса, освобождает ресурсы
        /// </summary>
        ~SynchronizedCache()
        {
            if (cacheLock != null) cacheLock.Dispose();
        }
    }
}
