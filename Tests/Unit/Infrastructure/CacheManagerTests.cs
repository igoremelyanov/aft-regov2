using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AFT.RegoV2.Shared.Caching;
using AFT.RegoV2.Tests.Common.Base;

using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

#pragma warning disable 162 // unreachable code detected

namespace AFT.RegoV2.Tests.Unit.Infrastructure
{
    public class CacheManagerTests : AdminWebsiteUnitTestsBase
    {
        protected ICacheManager CacheManager;

        public override void BeforeEach()
        {
            base.BeforeEach();

            CacheManager = Container.Resolve<ICacheManager>();
        }

        [Test]
        public async Task CanCreateAndGetCachedStringByGuidAsync()
        {
            var id = Guid.NewGuid();
            
            var val1 = await CacheManager.GetOrCreateObjectAsync(CacheType.Generic, id, TimeSpan.MaxValue, () => Task.FromResult("@" + id));
            var val2 = await CacheManager.GetOrCreateObjectAsync(CacheType.Generic, id, TimeSpan.MaxValue, () =>
            {
                throw new Exception("Should never be called");
                return Task.FromResult("");
            });
            
            val1.Should().Be("@" + id);
            val1.Should().Be(val2);
        }

        [Test]
        public async Task CanCreateAndReCreateExpiredStringByGuidAsync()
        {
            Guid id = Guid.NewGuid();
            
            var val1 = await CacheManager.GetOrCreateObjectAsync(CacheType.Generic, id, TimeSpan.FromDays(-1), () => Task.FromResult("OLD" + id));
            var val2 = await CacheManager.GetOrCreateObjectAsync(CacheType.Generic, id, TimeSpan.MaxValue, () => Task.FromResult("NEW" + id));
            
            val1.Should().Be("OLD" + id);
            val2.Should().Be("NEW" + id);
        }
        
        [Test]
        public async Task CanCreateAndGetCachedGuidByStringAsync()
        {
            var id = Guid.NewGuid().ToString();
            
            var val1 = await CacheManager.GetOrCreateObjectAsync(CacheType.Test, id, TimeSpan.MaxValue, () => Task.FromResult(new Guid(id)));
            var val2 = await CacheManager.GetOrCreateObjectAsync(CacheType.Test, id, TimeSpan.MaxValue, () =>
            {
                throw new Exception("Should never be called");
                return Task.FromResult(Guid.Empty);
            });
            
            val1.Should().Be(new Guid(id));
            val1.Should().Be(val2);
        }

        [Test]
        public async Task CanCreateAndReCreateExpiredGuidByStringAsync()
        {
            var id1 = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            
            var val1 = await CacheManager.GetOrCreateObjectAsync(CacheType.Test, id1, TimeSpan.FromDays(-1), () => Task.FromResult(new Guid(id1))); 
            var val2 = await CacheManager.GetOrCreateObjectAsync(CacheType.Test, id2, TimeSpan.MaxValue, () => Task.FromResult(new Guid(id2)));
            
            val1.Should().Be(new Guid(id1));
            val2.Should().Be(new Guid(id2));
        }

        [Test]
        public async Task GettingNewvalueAfterExpiredAsync()
        {
            var id = Guid.NewGuid().ToString();
            
            var val1 = await CacheManager.GetOrCreateObjectAsync(CacheType.Test, id, TimeSpan.FromMilliseconds(500), () => Task.FromResult("old"));
            Thread.Sleep(700); 
            var val2 = await CacheManager.GetOrCreateObjectAsync(CacheType.Test, id, TimeSpan.FromSeconds(1), () => Task.FromResult("new"));
            
            val1.Should().Be("old");
            val2.Should().Be("new");
        }

        [Test]
        public async Task CanGetCacheStats()
        {
            var id1 = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var id3 = Guid.NewGuid().ToString();

            await CacheManager.GetOrCreateObjectAsync(CacheType.Test, id1, TimeSpan.MaxValue, () => Task.FromResult(new Guid(id1)));
            await CacheManager.GetOrCreateObjectAsync(CacheType.Test, id2, TimeSpan.MaxValue, () => Task.FromResult(new Guid(id2)));
            await CacheManager.GetOrCreateObjectAsync(CacheType.Generic, id3, TimeSpan.MaxValue, () => Task.FromResult(new Guid(id2)));
            var stats = await CacheManager.GetCacheStatsAsync();

            stats.Keys.Single(x => x.Type == CacheType.Any).Number.Should().Be(0);
            stats.Keys.Single(x => x.Type == CacheType.Generic).Number.Should().Be(1);
            stats.Keys.Single(x => x.Type == CacheType.Test).Number.Should().Be(2);
        }
    }
}
