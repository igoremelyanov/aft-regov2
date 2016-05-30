using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data.Users;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Security.Interfaces
{
    public interface IAdminQueries : IApplicationService
    {
        IQueryable<Admin> GetAdmins();
        Admin GetAdminById(Guid adminId);
        Admin GetAdminByName(string username);
        ValidationResult GetValidationResult(LoginAdmin data);
        ValidationResult GetValidationResult(AddAdminData data);
        ValidationResult GetValidationResult(EditAdminData data);
        /// <summary>
        /// Gets current admin licensee filters
        /// </summary>
        IEnumerable<Guid> GetLicenseeFilterSelections();
        /// <summary>
        /// Gets current admin brand filters
        /// </summary>
        IEnumerable<Guid> GetBrandFilterSelections();
        bool IsBrandAllowed(Guid adminId, Guid brandId);
    }
}