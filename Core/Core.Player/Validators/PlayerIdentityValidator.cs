using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Data.Player.Enums;
using IdentificationDocumentSettings = AFT.RegoV2.Core.Player.Data.IdentificationDocumentSettings;

namespace AFT.RegoV2.Core.Player.Validators
{
    public class PlayerIdentityValidator : IPlayerIdentityValidator
    {
        private readonly IPlayerRepository _playerRepository;

        public PlayerIdentityValidator()
        {

        }

        public PlayerIdentityValidator(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public void Validate(Guid playerId, TransactionType transactionType)
        {
            var player = _playerRepository.Players
                .Include(o => o.IdentityVerifications)
                .Single(o => o.Id == playerId);

            var setting = _playerRepository.IdentificationDocumentSettings
                .SingleOrDefault(o => o.BrandId == player.BrandId
                && o.TransactionType == transactionType);

            if (setting == null)
                return;

            ValidatePlayersSetting(setting, player);
        }

        private static void ValidatePlayersSetting(IdentificationDocumentSettings setting, Common.Data.Player.Player player)
        {
            if (setting.IdFront)
            {
                var uploading = player.IdentityVerifications
                    .FirstOrDefault(o => o.DocumentType == DocumentType.Id && o.VerificationStatus == VerificationStatus.Verified);

                if (uploading == null || uploading.FrontFile == null)
                    throw new Exception("Player's identification documents doesn't match configured criteria");
            }

            if (setting.IdBack)
            {
                var uploading = player.IdentityVerifications
                    .FirstOrDefault(o => o.DocumentType == DocumentType.Id && o.VerificationStatus == VerificationStatus.Verified);

                if (uploading == null || uploading.BackFile == null)
                    throw new Exception("Player's identification documents doesn't match configured criteria");
            }

            if (setting.CreditCardFront)
            {
                var uploading = player.IdentityVerifications
                    .FirstOrDefault(o => o.DocumentType == DocumentType.CreditCard && o.VerificationStatus == VerificationStatus.Verified);

                if (uploading == null || uploading.FrontFile == null)
                    throw new Exception("Player's identification documents doesn't match configured criteria");
            }

            if (setting.CreditCardBack)
            {
                var uploading = player.IdentityVerifications
                    .FirstOrDefault(o => o.DocumentType == DocumentType.CreditCard && o.VerificationStatus == VerificationStatus.Verified);

                if (uploading == null || uploading.BackFile == null)
                    throw new Exception("Player's identification documents doesn't match configured criteria");
            }

            if (setting.POA)
            {
                var uploading = player.IdentityVerifications
                    .FirstOrDefault(o => o.DocumentType == DocumentType.POA && o.VerificationStatus == VerificationStatus.Verified);

                if (uploading == null || uploading.FrontFile == null)
                    throw new Exception("Player's identification documents doesn't match configured criteria");
            }

            if (setting.DCF)
            {
                var uploading = player.IdentityVerifications
                    .FirstOrDefault(o => o.DocumentType == DocumentType.DCF && o.VerificationStatus == VerificationStatus.Verified);

                if (uploading == null || uploading.FrontFile == null)
                    throw new Exception("Player's identification documents doesn't match configured criteria");
            }
        }
    }

    public interface IPlayerIdentityValidator
    {
        void Validate(Guid playerId, TransactionType transactionType);
    }
}
