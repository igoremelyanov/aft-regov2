using System.Net;
using AFT.RegoV2.Core.Common.Data.Admin;
using FluentValidation;

namespace AFT.RegoV2.Core.Security.Validators.IpRegulations
{
    public class AddBrandIpRegulationValidator : AbstractValidator<AddBrandIpRegulationData>
    {
        public AddBrandIpRegulationValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            ValidateIpAddress();
        }

        public void ValidateIpAddress()
        {
            RuleFor(x => x.IpAddress)
                .NotEmpty()
                .WithMessage(BrandIpRegulationResponseCodes.Required.ToString())
                .Must(x =>
                {
                    IPAddress address;
                    return IPAddress.TryParse(x, out address);
                })
                .WithMessage(BrandIpRegulationResponseCodes.IpAddressInvalid.ToString());
        }
    }
}
