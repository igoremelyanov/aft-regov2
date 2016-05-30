using System;

using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Providers;
using AFT.RegoV2.GameApi.Tests.Core;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AFT.RegoV2.GameApi.Tests.Unit.Services
{
    [TestFixture]
    [Category("Unit")]
    public class TokenProviderTests : MoqUnitTestBase
    {
        [Test]
        public void Can_Encrypt_Token()
        {
            var playerId = Guid.NewGuid();
            ITokenProvider tp = Create<TokenProvider, ITokenProvider>();

            var token = tp.Encrypt(playerId);

            var encrypted = playerId.ToString();
            token.Should().Be(encrypted);
        }

        [Test]
        public void Can_Decrypt_Token()
        {
            var playerId = Guid.NewGuid();
            var encrypted = playerId.ToString();
            ITokenProvider tp = Create<TokenProvider, ITokenProvider>();

            var decrypted = tp.Decrypt(encrypted);

            decrypted.Should().Be(playerId);
        }
    }
}