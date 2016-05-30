using System.Linq;
using AFT.RegoV2.Core.Auth;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.MemberApi.Interface.Exceptions;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.Tests.Common;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Player
{
    internal class ChangePasswordTests : PlayerServiceTestsBase
    {
        private RegisterRequest _registrationData;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _registrationData = RegisterPlayer().Result;
        }

        [Test]
        public async void Can_change_password()
        {
            var authRepository = Container.Resolve<IAuthRepository>();
            var initialPasswordValue = authRepository.Actors.Last().EncryptedPassword;
            await PlayerWebservice.ChangePasswordAsync(new ChangePasswordRequest
            {
                Username = _registrationData.Username,
                OldPassword = _registrationData.Password,
                NewPassword = "newPa$$word"
            });

            var changedPasswordValue = authRepository.Actors.Last().EncryptedPassword;
            //Assert.False(result.IsErrorResponse());
            Assert.AreNotEqual(initialPasswordValue, changedPasswordValue);
        }

        [Test]
        public void Empty_username_validation_works()
        {
            var e = Assert.Throws<MemberApiValidationException>(
                        async () => await PlayerWebservice.ChangePasswordAsync(new ChangePasswordRequest { Username = string.Empty, OldPassword = _registrationData.Password, NewPassword = _registrationData.Password }));

            Assert.AreEqual(PlayerAccountResponseCode.UsernameShouldNotBeEmpty.ToString(), e.Message);
        }

        [Test]
        public void Empty_password_validation_works()
        {
            var e = Assert.Throws<MemberApiValidationException>(
                        async () => await PlayerWebservice.ChangePasswordAsync(new ChangePasswordRequest { Username = _registrationData.Username, OldPassword = _registrationData.Password, NewPassword = string.Empty }));

            Assert.AreEqual(PlayerAccountResponseCode.PasswordShouldNotBeEmpty.ToString(), e.Message);
        }

        [Test]
        [TestCase("short")]
        [TestCase("too_long_to_be_real_password")]
        public void Password_length_validation_works(string password)
        {
            var e = Assert.Throws<MemberApiValidationException>(
                    async () => await PlayerWebservice.ChangePasswordAsync(new ChangePasswordRequest { Username = _registrationData.Username, OldPassword = _registrationData.Password, NewPassword = password }));

            Assert.IsNotEmpty(e.Message);
        }

        [Test]
        public void Player_is_absent_validation_works()
        {
            var e = Assert.Throws<MemberApiValidationException>(
                    async () => await PlayerWebservice.ChangePasswordAsync(new ChangePasswordRequest { Username = "random_username", OldPassword = _registrationData.Password, NewPassword = "random_pass" }));

            Assert.IsNotEmpty(e.Message);
            Assert.AreEqual(PlayerAccountResponseCode.PlayerDoesNotExist.ToString(), e.Message);
        }

        [Test]
        public void Old_password_validation_works()
        {
            var newPassword = TestDataGenerator.GetRandomString();
            string oldPasswordToEnter;
            do
            {
                oldPasswordToEnter = TestDataGenerator.GetRandomString();
            }
            while (oldPasswordToEnter == _registrationData.Password);

            var e = Assert.Throws<MemberApiValidationException>(
                        async () => await PlayerWebservice.ChangePasswordAsync(new ChangePasswordRequest { Username = _registrationData.Username, OldPassword = oldPasswordToEnter, NewPassword = newPassword }));

            Assert.IsNotEmpty(e.Message);
            Assert.AreEqual(PlayerAccountResponseCode.UsernamePasswordCombinationIsNotValid.ToString(), e.Message);
        }
    }
}
