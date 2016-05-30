using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Base
{
    [TestFixture]
    public abstract class TestBase
    {
        [TestFixtureSetUp]
        public virtual void BeforeAll()
        {
        }

        [SetUp]
        public virtual void BeforeEach()
        {
        }

        [TearDown]
        public virtual void AfterEach()
        {
        }

        [TestFixtureTearDown]
        public virtual void AfterAll()
        {
        }
    }
}