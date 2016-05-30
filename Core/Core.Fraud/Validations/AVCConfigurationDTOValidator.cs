using System.Linq;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using FluentValidation;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Fraud.Validations
{
    public class AVCConfigurationDTOValidator: AbstractValidator<AVCConfigurationDTO>
    {
        public AVCConfigurationDTOValidator(IFraudRepository fraudRepository, AvcConfigurationDtoQueriesEnum queryType)
        {
            switch (queryType)
            {
                case AvcConfigurationDtoQueriesEnum.Create:
                    Custom(config =>
                    {
                        return fraudRepository.AutoVerificationCheckConfigurations.Any(
                            record =>
                                record.BrandId == config.Brand &&
                                record.Currency == config.Currency &&
                                record.VipLevels.Select(o => o.Id).Intersect(config.VipLevels).Any())
                                    ? new ValidationFailure("", AVCConfigurationValidationMessagesEnum.RecordWithTheSameCompositeKeyAlreadyExists.ToString())
                                    : null;
                    }); break;

                case AvcConfigurationDtoQueriesEnum.Update:
                    Custom(config =>
                    {
                        return fraudRepository.AutoVerificationCheckConfigurations
                            .Where(record => record.Id != config.Id)
                            .Any(
                            record =>
                                record.BrandId == config.Brand &&
                                record.Currency == config.Currency &&
                                record.VipLevels.Select(o => o.Id).Intersect(config.VipLevels).Any()) ? new ValidationFailure("", AVCConfigurationValidationMessagesEnum.RecordWithTheSameCompositeKeyAlreadyExists.ToString()) : null;
                    });
                    break;
            }         
        }
    }

    public enum AvcConfigurationDtoQueriesEnum
    {
        Create,
        Update
    }
}
