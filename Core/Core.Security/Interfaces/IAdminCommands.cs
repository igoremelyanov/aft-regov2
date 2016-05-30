using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data.Users;

namespace AFT.RegoV2.Core.Security.Interfaces
{
    public interface IAdminCommands: IApplicationService
    {
        Admin CreateAdmin(AddAdminData data);
        void UpdateAdmin(EditAdminData data);
        void ChangePassword(AdminId adminId, string password);
        void Activate(ActivateUserData adminId);
        void Deactivate(DeactivateUserData adminId);
        void AddBrandToAdmin(Guid adminId, Guid brandId);
        void RemoveBrandFromAdmin(Guid adminId, Guid brandId);
        /// <summary>
        /// Sets current admin licensee filter selection
        /// </summary>
        void SetLicenseeFilterSelections(IEnumerable<Guid> selectedLicensees);
        /// <summary>
        /// Sets current admin brand filter selection
        /// </summary>
        void SetBrandFilterSelections(IEnumerable<Guid> selectedBrands);
    }
}