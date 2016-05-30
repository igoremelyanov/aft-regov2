﻿using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Extensions;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using FluentValidation;

namespace AFT.RegoV2.Core.Payment.Validators.Bank
{
    public class EditBankValidator : AbstractValidator<EditBankData>
    {
        private readonly IPaymentRepository _paymentRepository;

        public EditBankValidator(IPaymentRepository paymentRepository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            _paymentRepository = paymentRepository;

            ValidateId();
            ValidateBrandId();
            ValidateBankName();
            ValidateCountryCode();
            ValidateRemarks();
        }

        private void ValidateId()
        {
            RuleFor(x => x.Id)
                .Must(x => x != Guid.Empty)
                .WithMessage(BankValidationError.Required)
                .Must(x =>
                {
                    var bank = _paymentRepository.Banks.SingleOrDefault(y => y.Id == x);
                    return bank != null;
                })
                .WithMessage(BankValidationError.UnknownId);
        }

        private void ValidateBrandId()
        {
            RuleFor(x => x.BrandId)
                .Must(x => x != Guid.Empty)
                .WithMessage(BankValidationError.Required)
                .Must(x =>
                {
                    var brand = _paymentRepository.Brands.SingleOrDefault(y => y.Id == x);
                    return brand != null;
                })
                .WithMessage(BankValidationError.UnknownBrand);
        }

        private void ValidateBankName()
        {
            RuleFor(x => x.BankName)
                .NotEmpty()
                .WithMessage(BankValidationError.Required)
                .Length(1, Common.BankNameMaxLength)
                .WithMessage(BankValidationError.MaxLength50)
                .Matches(RegularExpression.AlphaNumericDashUnderscoreSpace)
                .WithMessage(BankValidationError.AlphanumericDashUnderscoreSpace);
        }

        private void ValidateCountryCode()
        {
            RuleFor(x => x.CountryCode)
                .NotEmpty()
                .WithMessage(BankValidationError.Required)
                .Must(x => _paymentRepository.Countries.Any(y => y.Code == x))
                .WithMessage(BankValidationError.UnknownCountry);
        }

        private void ValidateRemarks()
        {
            RuleFor(x => x.Remarks)
                .NotEmpty()
                .WithMessage(BankValidationError.Required)
                .Length(1, Common.RemarksMaxLength)
                .WithMessage(BankValidationError.MaxLength200);
        }
    }
}