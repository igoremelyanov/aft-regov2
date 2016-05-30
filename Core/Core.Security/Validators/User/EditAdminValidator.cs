using AFT.RegoV2.Core.Common.Data.Security.Users;

namespace AFT.RegoV2.Core.Security.Validators.User
{
    public class EditAdminValidator : UserValidatorBase<EditAdminData>
    {
        public EditAdminValidator()
        {
            ValidateUsername();

            ValidateFirstName();

            ValidateLastName();

            ValidateAssignedLicensees();

            ValidateAllowedBrands();

            ValidateCurrencies();
        }
    }
}
