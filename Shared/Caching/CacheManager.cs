using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace AFT.RegoV2.Shared.Caching
{
    public sealed class CacheManager : ICacheManager
    {
        private readonly ICacheManager _cacheManager;
        // one day into the future
        private static readonly TimeSpan MaxTimeSpan = new TimeSpan((long)24 * 60 * 60 * TimeSpan.TicksPerSecond);
        // every 30 minutes check for expired
        private static readonly TimeSpan MinExpirationCheckTime = new TimeSpan((long)30 * 60 * TimeSpan.TicksPerSecond);

        private static readonly object CleanUpExpiredLock = new object();
        private static readonly object DeleteKeysLock = new object();

        private readonly ConcurrentDictionary<string, Tuple<DateTime, object>> _objectByString =
            new ConcurrentDictionary<string, Tuple<DateTime, object>>(StringComparer.OrdinalIgnoreCase);

        private DateTime _lastCheckForExpired = DateTime.MinValue;

        public CacheManager()
        {
            _cacheManager = this;
        }

        Task<T> ICacheManager.GetOrCreateObjectAsync<T>(object cacheKey, TimeSpan? expiresIn, Func<Task<T>> getFunc)
        {
            return _cacheManager.GetOrCreateObjectAsync(CacheType.Generic, cacheKey, expiresIn, getFunc);
        }

        async Task<T> ICacheManager.GetOrCreateObjectAsync<T>(CacheType cacheType, object cacheKey, TimeSpan? expiresIn, Func<Task<T>> getFunc)
        {
            if (cacheType == CacheType.Any) cacheType = CacheType.Generic;
            var key = string.Concat(((int)cacheType), '_', cacheKey);

            Tuple<DateTime, object> tuple;
            if (!_objectByString.TryGetValue(key, out tuple) || tuple.Item1 < DateTime.UtcNow)
            {
                // expiresIn.Value > MaxTimeSpan is needed to avoid overflow exception if for example you pass DateTime.MaxValue
                if (expiresIn == null || expiresIn.Value > MaxTimeSpan) expiresIn = MaxTimeSpan;
                var val = await getFunc();
                var now = DateTime.UtcNow;
                var expirationTime = now.Add(expiresIn.Value);
                tuple = Tuple.Create(expirationTime, (object)val);
                _objectByString.AddOrUpdate(key, tuple, (k, v) => tuple);
            }
            CheckIfNeedToCleanUp();
            return (T)tuple.Item2;
        }

        T ICacheManager.GetOrCreateObject<T>(object cacheKey, TimeSpan? expiresIn, Func<T> getFunc)
        {
            return _cacheManager.GetOrCreateObject(CacheType.Generic, cacheKey, expiresIn, getFunc);
        }

        T ICacheManager.GetOrCreateObject<T>(CacheType cacheType, object cacheKey, TimeSpan? expiresIn, Func<T> getFunc)
        {
            if (cacheType == CacheType.Any) cacheType = CacheType.Generic;
            var key = string.Concat(((int)cacheType), '_', cacheKey);

            Tuple<DateTime, object> tuple;
            if (!_objectByString.TryGetValue(key, out tuple) || tuple.Item1 < DateTime.UtcNow)
            {
                // expiresIn.Value > MaxTimeSpan is needed to avoid overflow exception if for example you pass DateTime.MaxValue
                if (expiresIn == null || expiresIn.Value > MaxTimeSpan) expiresIn = MaxTimeSpan;
                var val = getFunc();
                var now = DateTime.UtcNow;
                var expirationTime = now.Add(expiresIn.Value);
                tuple = Tuple.Create(expirationTime, (object)val);
                _objectByString.AddOrUpdate(key, tuple, (k, v) => tuple);
            }
            CheckIfNeedToCleanUp();
            return (T)tuple.Item2;
        }

        private void CheckIfNeedToCleanUp()
        {
            if (_lastCheckForExpired < DateTime.UtcNow)
            {
                lock (CleanUpExpiredLock)
                {
                    if (_lastCheckForExpired < DateTime.UtcNow)
                    {
                        _lastCheckForExpired = DateTime.UtcNow.Add(MinExpirationCheckTime);
                        RemoveExpiredFromDict(_objectByString);
                    }
                }
            }
        }

        private static void RemoveExpiredFromDict<TKey, TValue>(ConcurrentDictionary<TKey, Tuple<DateTime, TValue>> dict)
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in dict)
            {
                if (kvp.Value.Item1 < now)
                {
                    Tuple<DateTime, TValue> val;
                    dict.TryRemove(kvp.Key, out val);
                }
            }
        }

        Task ICacheManager.ClearAsync(CacheType key)
        {
            if (key == CacheType.Any)
            {
                _objectByString.Clear();
            }
            else
            {
                lock (DeleteKeysLock)
                {
                    var prefix = String.Concat((int)key, '_');
                    foreach (var k in _objectByString.Keys)
                    {
                        if (k.StartsWith(prefix))
                        {
                            Tuple<DateTime, object> n;
                            _objectByString.TryRemove(k, out n);
                        }
                    }
                }
            }
            return Task.FromResult(true);
        }

        Task<CacheStats> ICacheManager.GetCacheStatsAsync()
        {
            var dict = Enum.GetValues(typeof(CacheType))
                .OfType<CacheType>()
                .ToDictionary(v => v, v => 0);

            char[] splitter = { '_' };
            foreach (var k in _objectByString.Keys)
            {
                var pre = k.Split(splitter, 2, StringSplitOptions.RemoveEmptyEntries)[0];
                byte found;
                if (byte.TryParse(pre, out found))
                {
                    var prefix = found + "_";
                    if (k.StartsWith(prefix))
                    {
                        ++dict[(CacheType)found];
                    }
                }
            }

            return Task.FromResult(new CacheStats
            {
                Keys = dict.Select(kvp =>
                    new CacheKeysByType
                    {
                        Type = kvp.Key,
                        Number = kvp.Value
                    }).ToArray()
            });
        }
    }
}