using System;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IBankQueries
    {
        ValidationResult ValidateCanAdd(AddBankData data);
        ValidationResult ValidateCanEdit(EditBankData data);
    }
}
