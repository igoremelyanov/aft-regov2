using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Security.Interfaces;

namespace AFT.RegoV2.Core.Security.Validators.User
{
    public class AddAdminValidator : UserValidatorBase<AddAdminData>
    {
        public AddAdminValidator(ISecurityRepository repository)
        {
            ValidateUsernameUnique(repository);

            ValidateUsername();

            ValidateFirstName();

            ValidateLastName();

            ValidatePassword();

            ValidateAssignedLicensees();

            ValidateAllowedBrands();

            ValidateCurrencies();
        }
    }
}
