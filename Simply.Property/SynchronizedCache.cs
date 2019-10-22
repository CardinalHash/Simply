using System;
using System.Collections.Generic;
using System.Threading;

namespace Simply.Property
{
    internal class SynchronizedCache<Key, Value>
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private Dictionary<Key, Value> innerCache = new Dictionary<Key, Value>();
        public int Count => innerCache.Count;
        public Value GetOrCreate(Key key, Func<Value> value)
        {
            cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (innerCache.ContainsKey(key)!= true)
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        innerCache.Add(key, value());
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
        ~SynchronizedCache()
        {
            if (cacheLock != null) cacheLock.Dispose();
        }
    }
}
