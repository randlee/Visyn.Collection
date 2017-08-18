﻿using System;
using System.Linq;
using NUnit.Framework;

namespace Visyn.Collection.Test
{
    [TestFixture]
    public static class SimpleCacheTests
    {
        public static readonly Func<string, int> TestStrlenFunc = key => key.Length;

        public static readonly Func<string, string> TestLengthStringFn = (input) => input.ToLower() == "null" ? null : input.Length.ToString();

        public static readonly Func<string, string> TestNullFunc = null;

        public static void CacheGet(this ICache<string, string> cache, string item)
        {
            bool contains = cache.ContainsKey(item);
            int countPrior = cache.Count;
            var result = cache.Get(item);
            if (item.ToLower() == "null") Assert.IsNull(result);
            else Assert.AreEqual(item.Length.ToString(), result);
            Assert.AreEqual(cache.Count, countPrior + (contains ? 0 : 1));
        }

        public static void CacheGet(this ICache<string, int> cache, string item)
        {
            bool contains = cache.ContainsKey(item);
            int countPrior = cache.Count;
            var result = cache.Get(item);
            Assert.AreEqual(item.Length, result);
            Assert.AreEqual(cache.Count, countPrior + (contains ? 0 : 1));
        }

        public static void VerifyCacheCount<TKey,TValue>(this ICache<TKey, TValue> cache, int expectedCount)
        {
            Assert.AreEqual(expectedCount, cache.Count);
            Assert.AreEqual(expectedCount, cache.Keys.Count());
            Assert.AreEqual(expectedCount, cache.Values.Count());
        }

        public static void VerifyCacheCount<TKey, TValue>(this ICacheHitStats cache, int expectedCount, int hits, int misses, int notFound)
        {
            ((ICache<TKey,TValue>)cache).VerifyCacheCount(expectedCount );
            Assert.AreEqual(expectedCount, cache.Entries);
            Assert.AreEqual(hits, cache.Hits,$"Hits: Expected:{hits} Actual:{cache.Hits}");
            Assert.AreEqual(misses, cache.Misses, $"Hits: Expected:{misses} Actual:{cache.Misses}");
            Assert.AreEqual(notFound, cache.NotFound, $"Hits: Expected:{notFound} Actual:{cache.NotFound}");
        }


        [Test]
        public static void StringStringTest()
        {
            var cache = new SimpleCache<string, string>(TestLengthStringFn);

            Assert.AreEqual(cache.GetMissingCacheItem, TestLengthStringFn);
            cache.VerifyCacheCount( 0);
            cache.CacheGet( "first");
            cache.VerifyCacheCount( 1);
            cache.CacheGet("second");
            cache.VerifyCacheCount( 2);
            cache.CacheGet("first");
            cache.VerifyCacheCount( 2);
            cache.CacheGet("null");
            cache.VerifyCacheCount( 3);
            Assert.Throws<ArgumentNullException>(() => cache.Get(null));
            cache.VerifyCacheCount( 3);
            cache.CacheGet("fourth");
            cache.VerifyCacheCount(4);
            cache.CacheGet("first");
            cache.VerifyCacheCount(4);
        }



        [Test]
        public static void SimpleCacheTest()
        {
            var cache = new SimpleCache<string, int>(TestStrlenFunc);

            cache.VerifyCacheCount(0);

            Assert.AreEqual(cache.Count, 0);
            cache.CacheGet("first");
            cache.VerifyCacheCount(1);
            Assert.Throws<ArgumentNullException>(() => cache.Get(null));
            cache.CacheGet("second");
            cache.VerifyCacheCount(2);
            cache.CacheGet("second");
            cache.VerifyCacheCount(2);
            cache.CacheGet("third");
            cache.VerifyCacheCount(3);

            Assert.AreEqual(cache["first"], "first".Length);
            Assert.AreEqual(cache["second"], "second".Length);
        }

        [Test]
        public static void NullConstructorTest()
        {   // No delegate, nothing is added to the cache
            var cacheNull = new SimpleCache<string, int>((Func<string, int>) null);
            Assert.AreEqual(cacheNull.Count, 0);
            Assert.AreEqual(cacheNull.Get("first"), 0);
            Assert.AreEqual(cacheNull.Count, 0);
            Assert.Throws<ArgumentNullException>(() => cacheNull.Get(null));
            Assert.AreEqual(cacheNull.Get("second"), 0);
            Assert.AreEqual(cacheNull.Count, 0);
        }

        [Test]
        public static void NullFunctionTest()
        {
            // Delegate return null, nothing is added to the cache
            var cache = new SimpleCache<string, string>(TestNullFunc);
            Assert.AreEqual(cache.Count, 0);
            Assert.AreEqual(cache.Get("first"), null);
            Assert.AreEqual(cache.Count, 0);
            Assert.AreEqual(cache.Get("second"), null);
            Assert.AreEqual(cache.Count, 0);
            Assert.Throws<ArgumentNullException>(() => cache.Get(null));
            Assert.AreEqual(cache.Get(3.ToString()), null);
            Assert.AreEqual(cache.Count, 0);
        }


        [Test]
        public static void HitCountersTest( )
        {
            var cache = new SimpleCache<string, string>(TestLengthStringFn);

            Assert.AreEqual(cache.GetMissingCacheItem, TestLengthStringFn);
            cache.VerifyCacheCount<string, string>(0,0,0,0);
            cache.CacheGet("first");
            cache.VerifyCacheCount<string, string>(1,0,1,0);
            cache.CacheGet("second");
            cache.VerifyCacheCount<string, string>(2,0,2,0);
            cache.CacheGet("first");
            cache.VerifyCacheCount<string, string>(2,1,2,0);
            cache.CacheGet("null");
            cache.VerifyCacheCount<string, string>(3,1,3,1);
            Assert.Throws<ArgumentNullException>(() => cache.Get(null));
            cache.VerifyCacheCount<string, string>(3,1,3,1);
            cache.CacheGet("fourth");
            cache.VerifyCacheCount<string, string>(4,1,4,1);
            cache.CacheGet("first");
            cache.VerifyCacheCount<string, string>(4,2,4,1);
        }
    }
}