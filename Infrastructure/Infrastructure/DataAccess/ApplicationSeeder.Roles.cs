using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;
using AFT.RegoV2.Core.Auth.Data;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Common.Data.Security.Roles;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Shared.Utils;
using Role = AFT.RegoV2.Core.Security.Data.Users.Role;

namespace AFT.RegoV2.Infrastructure.DataAccess
{
    public partial class ApplicationSeeder
    {
        private void CreateSystemActor()
        {
            var systemId = RoleIds.SystemId;
            if (_authRepository.Actors.Any(a => a.Id == systemId))
                return;

            _authCommands.CreateRole(new CreateRole
            {
                RoleId = systemId,
                Permissions = _authRepository.Permissions.Select(p => p.Id).ToList()
            });
            _authCommands.CreateActor(new CreateActor
            {
                ActorId = systemId,
                Username = "System",
                Password = "System"
            });
            _authCommands.AssignRoleToActor(new AssignRole
            {
                ActorId = systemId,
                RoleId = systemId
            });
        }

        private void CreateSuperAdmin(IEnumerable<Guid> licenseeIds, IEnumerable<Guid> brandIds)
        {
            var adminId = RoleIds.SuperAdminId;
            if (_securityRepository.Admins.Any(a => a.Id == adminId))
                return;

            var superAdmin = "SuperAdmin";

            var role = new Role
            {
                Id = adminId,
                Code = superAdmin,
                Name = superAdmin,
                CreatedDate = DateTime.UtcNow
            };

            role.SetLicensees(licenseeIds);

            var admin = new Admin
            {
                Id = adminId,
                Username = superAdmin,
                FirstName = superAdmin,
                LastName = superAdmin,
                IsActive = true,
                Description = superAdmin,
                Role = role
            };

            admin.SetLicensees(licenseeIds);

            foreach (var licenseeId in licenseeIds)
            {
                admin.LicenseeFilterSelections.Add(new LicenseeFilterSelection
                {
                    AdminId = admin.Id,
                    LicenseeId = licenseeId,
                    Admin = admin
                });
            }

            admin.SetAllowedBrands(brandIds);

            foreach (var brandId in brandIds)
            {
                admin.BrandFilterSelections.Add(new BrandFilterSelection
                {
                    AdminId = admin.Id,
                    BrandId = brandId,
                    Admin = admin
                });
            }

            _securityRepository.Admins.AddOrUpdate(admin);
            _securityRepository.SaveChanges();

            _authCommands.CreateRole(new CreateRole
            {
                RoleId = adminId,
                Permissions = _authRepository.Permissions.Select(p => p.Id).ToList()
            });
            _authCommands.CreateActor(new CreateActor
            {
                ActorId = admin.Id,
                Username = admin.Username,
                Password = admin.Username
            });
            _authCommands.AssignRoleToActor(new AssignRole
            {
                ActorId = adminId,
                RoleId = adminId
            });
        }

        private void CreateFraudOfficerRole(List<Guid> licenseeIds)
        {
            if (_roleService.GetRoles().Any(role => role.Name == "FraudOfficer"))
                return;

            var permissions = new List<Permission>
            {
                new Permission {Name = Permissions.View, Module = Modules.OfflineDepositRequests},
                new Permission {Name = Permissions.View, Module = Modules.OfflineDepositConfirmation},
                new Permission {Name = Permissions.View, Module = Modules.DepositVerification},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalRequest},
                new Permission {Name = Permissions.Exempt, Module = Modules.OfflineWithdrawalExemption},
                new Permission {Name = Permissions.Pass, Module = Modules.OfflineWithdrawalWagerCheck},
                new Permission {Name = Permissions.Fail, Module = Modules.OfflineWithdrawalWagerCheck},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalWagerCheck},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalVerification},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalAcceptance},
                new Permission {Name = Permissions.Verify, Module = Modules.OfflineWithdrawalVerification},
                new Permission {Name = Permissions.Unverify, Module = Modules.OfflineWithdrawalVerification},
                new Permission {Name = Permissions.Reject, Module = Modules.OfflineWithdrawalVerification},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalOnHold},
                new Permission {Name = Permissions.Pass, Module = Modules.OfflineWithdrawalInvestigation},
                new Permission {Name = Permissions.Fail, Module = Modules.OfflineWithdrawalInvestigation},
                new Permission {Name = Permissions.View, Module = Modules.RiskProfileCheckConfiguration},
                new Permission {Name = Permissions.Create, Module = Modules.RiskProfileCheckConfiguration},
                new Permission {Name = Permissions.Update, Module = Modules.RiskProfileCheckConfiguration},
                new Permission {Name = Permissions.View, Module = Modules.FraudManager},
                new Permission {Name = Permissions.Create, Module = Modules.FraudManager},
                new Permission {Name = Permissions.Update, Module = Modules.FraudManager},
                new Permission {Name = Permissions.Activate, Module = Modules.FraudManager},
                new Permission {Name = Permissions.Deactivate, Module = Modules.FraudManager},
                new Permission {Name = Permissions.View, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.Create, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.Update, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.Activate, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.Deactivate, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.View, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.Create, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.Update, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.View, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Search, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Activate, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Deactivate, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.View, Module = Modules.PlayerReport},
                new Permission {Name = Permissions.Export, Module = Modules.PlayerReport},
                new Permission {Name = Permissions.View, Module = Modules.PlayerBetHistoryReport},
                new Permission {Name = Permissions.Export, Module = Modules.PlayerBetHistoryReport},
                new Permission {Name = Permissions.View, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Create, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Update, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Activate, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.Deactivate, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.View, Module = Modules.ResponsibleGambling},
                new Permission {Name = Permissions.Activate, Module = Modules.ResponsibleGambling},
                new Permission {Name = Permissions.Deactivate, Module = Modules.ResponsibleGambling},
                new Permission {Name = Permissions.Create, Module = Modules.SignUpFraudTypes},
                new Permission {Name = Permissions.Update, Module = Modules.SignUpFraudTypes},
                new Permission {Name = Permissions.View, Module = Modules.SignUpFraudTypes},
                new Permission {Name = Permissions.Revert, Module = Modules.OfflineWithdrawalAcceptance},
                new Permission {Name = Permissions.View, Module = Modules.DuplicateConfiguration},
                new Permission {Name = Permissions.Create, Module = Modules.DuplicateConfiguration},
                new Permission {Name = Permissions.Update, Module = Modules.DuplicateConfiguration},
            };

            var permissionIds =
                _authRepository.Permissions
                    .ToList()
                    .Where(p => permissions.Any(perm => perm.Name == p.Name && perm.Module == p.Module))
                    .Select(p => p.Id)
                    .ToList();

            _roleService.CreateRole(new AddRoleData
            {
                Name = "FraudOfficer",
                Code = "FraudOfficer",
                CheckedPermissions = permissionIds,
                AssignedLicensees = licenseeIds
            });
        }

        private void CreateBrandManagerRole(IList<Guid> licenseeIds)
        {
            const string roleName = "BrandManager";

            if (_roleService.GetRoles().Any(role => role.Name == roleName))
                return;

            var permissions = new List<Permission>
            {
                new Permission {Name = Permissions.View, Module = Modules.OfflineDepositRequests},
                new Permission {Name = Permissions.View, Module = Modules.OfflineDepositConfirmation},
                new Permission {Name = Permissions.Confirm, Module = Modules.OfflineDepositConfirmation},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalRequest},
                new Permission {Name = Permissions.Exempt, Module = Modules.OfflineWithdrawalExemption},
                new Permission {Name = Permissions.Pass, Module = Modules.OfflineWithdrawalWagerCheck},
                new Permission {Name = Permissions.Fail, Module = Modules.OfflineWithdrawalWagerCheck},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalWagerCheck},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalVerification},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalAcceptance},
                new Permission {Name = Permissions.Verify, Module = Modules.OfflineWithdrawalVerification},
                new Permission {Name = Permissions.Unverify, Module = Modules.OfflineWithdrawalVerification},
                new Permission {Name = Permissions.Reject, Module = Modules.OfflineWithdrawalVerification},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalOnHold},
                new Permission {Name = Permissions.Pass, Module = Modules.OfflineWithdrawalInvestigation},
                new Permission {Name = Permissions.Fail, Module = Modules.OfflineWithdrawalInvestigation},
                new Permission {Name = Permissions.View, Module = Modules.RiskProfileCheckConfiguration},
                new Permission {Name = Permissions.Create, Module = Modules.RiskProfileCheckConfiguration},
                new Permission {Name = Permissions.Update, Module = Modules.RiskProfileCheckConfiguration},
                new Permission {Name = Permissions.View, Module = Modules.FraudManager},
                new Permission {Name = Permissions.Create, Module = Modules.FraudManager},
                new Permission {Name = Permissions.Update, Module = Modules.FraudManager},
                new Permission {Name = Permissions.Activate, Module = Modules.FraudManager},
                new Permission {Name = Permissions.Deactivate, Module = Modules.FraudManager},
                new Permission {Name = Permissions.View, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.Create, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.Update, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.Activate, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.Deactivate, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.View, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.Create, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.Update, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.View, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Search, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Activate, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Deactivate, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.View, Module = Modules.PlayerReport},
                new Permission {Name = Permissions.Export, Module = Modules.PlayerReport},
                new Permission {Name = Permissions.View, Module = Modules.PlayerBetHistoryReport},
                new Permission {Name = Permissions.Export, Module = Modules.PlayerBetHistoryReport},
                new Permission {Name = Permissions.View, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Create, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Update, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Activate, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.Deactivate, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.View, Module = Modules.ResponsibleGambling},
                new Permission {Name = Permissions.Activate, Module = Modules.ResponsibleGambling},
                new Permission {Name = Permissions.Deactivate, Module = Modules.ResponsibleGambling},
                new Permission {Name = Permissions.Create, Module = Modules.SignUpFraudTypes},
                new Permission {Name = Permissions.Update, Module = Modules.SignUpFraudTypes},
                new Permission {Name = Permissions.View, Module = Modules.SignUpFraudTypes},
                new Permission {Name = Permissions.Revert, Module = Modules.OfflineWithdrawalAcceptance},
                new Permission {Name = Permissions.View, Module = Modules.DuplicateConfiguration},

                new Permission {Name = Permissions.Accept, Module = Modules.OfflineWithdrawalAcceptance}

            };

            AddPaymentPermissions(permissions);

            var permissionIds =
                _authRepository.Permissions
                    .ToList()
                    .Where(p => permissions.Any(
                        perm => perm.Name == p.Name && perm.Module == p.Module))
                    .Select(p => p.Id)
                    .ToList();

            _roleService.CreateRole(new AddRoleData
            {
                Name = roleName,
                Code = roleName,
                CheckedPermissions = permissionIds,
                AssignedLicensees = licenseeIds
            });
        }

        private void CreateKYCOfficerRole(List<Guid> licenseeIds)
        {
            if (_roleService.GetRoles().Any(role => role.Name == "KYCOfficer"))
                return;

            var permissions = new List<Permission>
            {
                new Permission {Name = Permissions.Search, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.View, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.View, Module = Modules.PlayerReport},
                new Permission {Name = Permissions.Export, Module = Modules.PlayerReport},
                new Permission {Name = Permissions.View, Module = Modules.PlayerBetHistoryReport},
                new Permission {Name = Permissions.Export, Module = Modules.PlayerBetHistoryReport},
                new Permission {Name = Permissions.View, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Create, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Update, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Verify, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Reject, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.View, Module = Modules.ResponsibleGambling},
                new Permission {Name = Permissions.Activate, Module = Modules.ResponsibleGambling},
                new Permission {Name = Permissions.Deactivate, Module = Modules.ResponsibleGambling},
                new Permission {Name = Permissions.View, Module = Modules.DuplicateConfiguration},
            };

            var permissionIds =
                _authRepository.Permissions
                    .ToList()
                    .Where(p => permissions.Any(perm => perm.Name == p.Name && perm.Module == p.Module))
                    .Select(p => p.Id)
                    .ToList();

            _roleService.CreateRole(new AddRoleData
            {
                Name = "KYCOfficer",
                Code = "KYCOfficer",
                CheckedPermissions = permissionIds,
                AssignedLicensees = licenseeIds
            });
        }

        private void CreateMarketingOfficerRole(IEnumerable<Guid> licenseeIds)
        {
            if (_roleService.GetRoles().Any(role => role.Name == "MarketingOfficer"))
                return;

            var permissions = new List<Permission>
            {
                new Permission {Name = Permissions.Create, Module = Modules.BonusManager},
                new Permission {Name = Permissions.Update, Module = Modules.BonusManager},
                new Permission {Name = Permissions.View, Module = Modules.BonusManager},
                new Permission {Name = Permissions.Activate, Module = Modules.BonusManager},
                new Permission {Name = Permissions.Deactivate, Module = Modules.BonusManager},
                new Permission {Name = Permissions.Create, Module = Modules.BonusTemplateManager},
                new Permission {Name = Permissions.Update, Module = Modules.BonusTemplateManager},
                new Permission {Name = Permissions.View, Module = Modules.BonusTemplateManager},
                new Permission {Name = Permissions.Delete, Module = Modules.BonusTemplateManager},
                new Permission {Name = Permissions.View, Module = Modules.GameManager},
                new Permission {Name = Permissions.View, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Search, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.View, Module = Modules.PlayerReport},
                new Permission {Name = Permissions.Export, Module = Modules.PlayerReport},
                new Permission {Name = Permissions.View, Module = Modules.PlayerBetHistoryReport},
                new Permission {Name = Permissions.Export, Module = Modules.PlayerBetHistoryReport},
                new Permission {Name = Permissions.View, Module = Modules.DuplicateConfiguration},
            };

            var permissionIds =
                _authRepository.Permissions
                    .ToList()
                    .Where(p => permissions.Any(perm => perm.Name == p.Name && perm.Module == p.Module))
                    .Select(p => p.Id)
                    .ToList();

            _roleService.CreateRole(new AddRoleData
            {
                Name = "MarketingOfficer",
                Code = "Marketing",
                CheckedPermissions = permissionIds,
                AssignedLicensees = licenseeIds.ToList()
            });
        }

        private void CreateCustomerServiceOfficerRole(IEnumerable<Guid> licenseeIds)
        {
            if (_roleService.GetRoles().Any(role => role.Name == "CSOfficer"))
                return;

            var permissions = new List<Permission>
            {
                new Permission {Name = Permissions.View, Module = Modules.BonusManager},
                new Permission {Name = Permissions.View, Module = Modules.BonusTemplateManager},
                new Permission {Name = Permissions.View, Module = Modules.OfflineDepositRequests},
                new Permission {Name = Permissions.Create, Module = Modules.OfflineDepositRequests},
                new Permission {Name = Permissions.View, Module = Modules.OfflineDepositConfirmation},
                new Permission {Name = Permissions.Confirm, Module = Modules.OfflineDepositConfirmation},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalRequest},
                new Permission {Name = Permissions.Create, Module = Modules.OfflineWithdrawalRequest},
                new Permission {Name = Permissions.View, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Search, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Create, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Update, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Activate, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Deactivate, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.AssignVipLevel, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.View, Module = Modules.PlayerReport},
                new Permission {Name = Permissions.Export, Module = Modules.PlayerReport},

                new Permission {Name = Permissions.ViewEmail, Module = Modules.PlayerReport},
                new Permission {Name = Permissions.ViewMobile, Module = Modules.PlayerReport},

                new Permission {Name = Permissions.View, Module = Modules.PlayerBetHistoryReport},
                new Permission {Name = Permissions.Export, Module = Modules.PlayerBetHistoryReport},
                new Permission {Name = Permissions.View, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Create, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Update, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.View, Module = Modules.IdentificationDocumentSettings},
                new Permission {Name = Permissions.Update, Module = Modules.IdentificationDocumentSettings},
                new Permission {Name = Permissions.Create, Module = Modules.IdentificationDocumentSettings},
                new Permission {Name = Permissions.View, Module = Modules.ResponsibleGambling},
                new Permission {Name = Permissions.Activate, Module = Modules.ResponsibleGambling},
                new Permission {Name = Permissions.Deactivate, Module = Modules.ResponsibleGambling},
                new Permission {Name = Permissions.View, Module = Modules.DuplicateConfiguration},
            };

            var permissionIds =
                _authRepository.Permissions
                    .ToList()
                    .Where(p => permissions.Any(perm => perm.Name == p.Name && perm.Module == p.Module))
                    .Select(p => p.Id)
                    .ToList();

            _roleService.CreateRole(new AddRoleData
            {
                Name = "CSOfficer",
                Code = "CSOfficer",
                CheckedPermissions = permissionIds,
                AssignedLicensees = licenseeIds.ToList()
            });
        }

        private void CreatePaymentOfficer(IEnumerable<Guid> licenseeIds)
        {
            if (_roleService.GetRoles().Any(role => role.Name == "PaymentOfficer"))
                return;

            var permissions = new List<Permission>
            {
                new Permission {Name = Permissions.View, Module = Modules.OfflineDepositRequests},
                new Permission {Name = Permissions.Create, Module = Modules.OfflineDepositRequests},
                new Permission {Name = Permissions.View, Module = Modules.OfflineDepositConfirmation},
                new Permission {Name = Permissions.Confirm, Module = Modules.OfflineDepositConfirmation},
                new Permission {Name = Permissions.Revert, Module = Modules.OfflineWithdrawalAcceptance},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalRequest},
                new Permission {Name = Permissions.Create, Module = Modules.OfflineWithdrawalRequest},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalVerification},
                new Permission {Name = Permissions.Verify, Module = Modules.OfflineWithdrawalVerification},
                new Permission {Name = Permissions.Unverify, Module = Modules.OfflineWithdrawalVerification},
                new Permission {Name = Permissions.Approve, Module = Modules.OfflineWithdrawalApproval},
                new Permission {Name = Permissions.Reject, Module = Modules.OfflineWithdrawalApproval},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalOnHold},
                new Permission {Name = Permissions.Search, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.View, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.AssignPaymentLevel, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.View, Module = Modules.PlayerReport},
                new Permission {Name = Permissions.Export, Module = Modules.PlayerReport},
                new Permission {Name = Permissions.View, Module = Modules.PlayerBetHistoryReport},
                new Permission {Name = Permissions.Export, Module = Modules.PlayerBetHistoryReport},
                new Permission {Name = Permissions.View, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Create, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Update, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.View, Module = Modules.ResponsibleGambling},
                new Permission {Name = Permissions.Activate, Module = Modules.ResponsibleGambling},
                new Permission {Name = Permissions.Deactivate, Module = Modules.ResponsibleGambling},

                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalAcceptance},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalWagerCheck},

                new Permission {Name = Permissions.Create, Module = Modules.BankAccounts},
                new Permission {Name = Permissions.Update, Module = Modules.BankAccounts},
                new Permission {Name = Permissions.View, Module = Modules.BankAccounts},
                new Permission {Name = Permissions.Activate, Module = Modules.BankAccounts},
                new Permission {Name = Permissions.Deactivate, Module = Modules.BankAccounts},

                new Permission {Name = Permissions.View, Module = Modules.DuplicateConfiguration},

                new Permission {Name = Permissions.Accept, Module = Modules.OfflineWithdrawalAcceptance},
                new Permission {Name = Permissions.View, Module = Modules.PaymentLevelSettings},
                new Permission {Name = Permissions.Update, Module = Modules.PaymentLevelSettings},
            };

            AddPaymentPermissions(permissions);

            var permissionIds =
                _authRepository.Permissions
                    .ToList()
                    .Where(p => permissions.Any(perm => perm.Name == p.Name && perm.Module == p.Module))
                    .Select(p => p.Id)
                    .ToList();

            _roleService.CreateRole(new AddRoleData
            {
                Name = "PaymentOfficer",
                Code = "Payment",
                CheckedPermissions = permissionIds,
                AssignedLicensees = licenseeIds.ToList()
            });
        }

        private void CreateDefaultRole(IEnumerable<Guid> licenseeIds)
        {
            if (_roleService.GetRoles().Any(role => role.Name == "Default"))
                return;

            _roleService.CreateRole(new AddRoleData
            {
                Name = "Default",
                Code = "Default",
                AssignedLicensees = licenseeIds.ToList(),
                CheckedPermissions = new List<Guid>()
            });
        }

        private void CreateLicenseeRole(IEnumerable<Guid> licenseeIds)
        {
            //TODO: this role should have predefined id for some code. Need to change this
            var roleId = RoleIds.LicenseeId;

            if (_roleService.GetRoles().Any(role => role.Name == "Licensee"))
                return;

            var permissions = new List<Permission>
            {
                new Permission {Name = Permissions.View, Module = Modules.LanguageManager},
                new Permission {Name = Permissions.Activate, Module = Modules.LanguageManager},
                new Permission {Name = Permissions.Deactivate, Module = Modules.LanguageManager},
                new Permission {Name = Permissions.View, Module = Modules.OfflineDepositRequests},
                new Permission {Name = Permissions.Create, Module = Modules.OfflineDepositRequests},
                new Permission {Name = Permissions.View, Module = Modules.OfflineDepositConfirmation},
                new Permission {Name = Permissions.Confirm, Module = Modules.OfflineDepositConfirmation},
                new Permission {Name = Permissions.View, Module = Modules.FraudManager},
                new Permission {Name = Permissions.Create, Module = Modules.FraudManager},
                new Permission {Name = Permissions.Update, Module = Modules.FraudManager},
                new Permission {Name = Permissions.Activate, Module = Modules.FraudManager},
                new Permission {Name = Permissions.Deactivate, Module = Modules.FraudManager},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalWagerCheck},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalAcceptance},
                new Permission {Name = Permissions.View, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.Create, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.Update, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.Activate, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.Deactivate, Module = Modules.WagerConfiguration},
                new Permission {Name = Permissions.View, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.Create, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.Update, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.Activate, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.Deactivate, Module = Modules.AutoVerificationConfiguration},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalRequest},
                new Permission {Name = Permissions.Create, Module = Modules.OfflineWithdrawalRequest},
                new Permission {Name = Permissions.Exempt, Module = Modules.OfflineWithdrawalExemption},
                new Permission {Name = Permissions.Pass, Module = Modules.OfflineWithdrawalWagerCheck},
                new Permission {Name = Permissions.Fail, Module = Modules.OfflineWithdrawalWagerCheck},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalVerification},
                new Permission {Name = Permissions.Verify, Module = Modules.OfflineWithdrawalVerification},
                new Permission {Name = Permissions.Unverify, Module = Modules.OfflineWithdrawalVerification},
                new Permission {Name = Permissions.Approve, Module = Modules.OfflineWithdrawalApproval},
                new Permission {Name = Permissions.Reject, Module = Modules.OfflineWithdrawalApproval},
                new Permission {Name = Permissions.View, Module = Modules.OfflineWithdrawalOnHold},
                new Permission {Name = Permissions.Pass, Module = Modules.OfflineWithdrawalInvestigation},
                new Permission {Name = Permissions.Fail, Module = Modules.OfflineWithdrawalInvestigation},
                new Permission {Name = Permissions.Create, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Update, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.View, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Activate, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.Deactivate, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.AssignVipLevel, Module = Modules.PlayerManager},
                new Permission {Name = Permissions.View, Module = Modules.PlayerReport},
                new Permission {Name = Permissions.Export, Module = Modules.PlayerReport},
                new Permission {Name = Permissions.View, Module = Modules.PlayerBetHistoryReport},
                new Permission {Name = Permissions.Export, Module = Modules.PlayerBetHistoryReport},
                new Permission {Name = Permissions.Create, Module = Modules.BrandManager},
                new Permission {Name = Permissions.Update, Module = Modules.BrandManager},
                new Permission {Name = Permissions.View, Module = Modules.BrandManager},
                new Permission {Name = Permissions.Activate, Module = Modules.BrandManager},
                new Permission {Name = Permissions.Deactivate, Module = Modules.BrandManager},
                new Permission {Name = Permissions.Create, Module = Modules.BonusManager},
                new Permission {Name = Permissions.Update, Module = Modules.BonusManager},
                new Permission {Name = Permissions.View, Module = Modules.BonusManager},
                new Permission {Name = Permissions.Activate, Module = Modules.BonusManager},
                new Permission {Name = Permissions.Deactivate, Module = Modules.BonusManager},
                new Permission {Name = Permissions.Create, Module = Modules.BonusTemplateManager},
                new Permission {Name = Permissions.Update, Module = Modules.BonusTemplateManager},
                new Permission {Name = Permissions.View, Module = Modules.BonusTemplateManager},
                new Permission {Name = Permissions.Delete, Module = Modules.BonusTemplateManager},
                new Permission {Name = Permissions.View, Module = Modules.GameManager},
                new Permission {Name = Permissions.View, Module = Modules.VipLevelManager},
                new Permission {Name = Permissions.Create, Module = Modules.VipLevelManager},
                new Permission {Name = Permissions.Update, Module = Modules.VipLevelManager},
                new Permission {Name = Permissions.Activate, Module = Modules.VipLevelManager},
                new Permission {Name = Permissions.Deactivate, Module = Modules.VipLevelManager},
                new Permission {Name = Permissions.View, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Create, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.Update, Module = Modules.PlayerBankAccount},
                new Permission {Name = Permissions.View, Module = Modules.IdentificationDocumentSettings},
                new Permission {Name = Permissions.Update, Module = Modules.IdentificationDocumentSettings},
                new Permission {Name = Permissions.Create, Module = Modules.IdentificationDocumentSettings},
                new Permission {Name =  Permissions.View, Module = Modules.RiskProfileCheckConfiguration},
                new Permission {Name =  Permissions.Create, Module = Modules.RiskProfileCheckConfiguration},
                new Permission {Name =  Permissions.Update, Module = Modules.RiskProfileCheckConfiguration},
                new Permission {Name = Permissions.Create, Module = Modules.SignUpFraudTypes},
                new Permission {Name = Permissions.Update, Module = Modules.SignUpFraudTypes},
                new Permission {Name = Permissions.View, Module = Modules.SignUpFraudTypes},
                new Permission {Name = Permissions.Revert, Module = Modules.OfflineWithdrawalAcceptance},

                new Permission {Name = Permissions.Create, Module = Modules.BankAccounts},
                new Permission {Name = Permissions.Update, Module = Modules.BankAccounts},
                new Permission {Name = Permissions.View, Module = Modules.BankAccounts},
                new Permission {Name = Permissions.Activate, Module = Modules.BankAccounts},
                new Permission {Name = Permissions.Deactivate, Module = Modules.BankAccounts},
                new Permission {Name = Permissions.View, Module = Modules.DuplicateConfiguration},

                new Permission {Name = Permissions.Accept, Module = Modules.OfflineWithdrawalAcceptance}
            };

            AddPaymentPermissions(permissions);

            var permissionIds =
                _authRepository.Permissions
                    .ToList()
                    .Where(p => permissions.Any(perm => perm.Name == p.Name && perm.Module == p.Module))
                    .Select(p => p.Id)
                    .ToList();

            _roleService.CreateRole(new AddRoleData
            {
                Name = "Licensee",
                Code = "Licensee",
                CheckedPermissions = permissionIds,
                AssignedLicensees = licenseeIds.ToList()
            });
        }

        public void PopulatePermissions()
        {
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName.StartsWith("AFT.RegoV2.")
                    && !x.FullName.StartsWith("AFT.RegoV2.Infrastructure"))
                .SelectMany(x => x.GetLoadableTypes())
                .Where(t => t.IsDescendentOf(typeof(IApplicationService)))
                .SelectMany(
                    service => service.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Select(method => method.GetCustomAttributes(typeof(PermissionAttribute), true))
                        .SelectMany(operationAttrs => operationAttrs.Select(o => (PermissionAttribute)o)
                        .Distinct()))
                .ForEach(p => RegisterPermission(p.Permission, p.Module));

            RegisterPermission(Permissions.View, Modules.BrandManager);
            RegisterPermission(Permissions.Create, Modules.BrandManager);
            RegisterPermission(Permissions.Update, Modules.BrandManager);
            RegisterPermission(Permissions.Activate, Modules.BrandManager);
            RegisterPermission(Permissions.Deactivate, Modules.BrandManager);

            RegisterPermission(Permissions.View, Modules.CurrencyManager);
            RegisterPermission(Permissions.Update, Modules.CurrencyManager);
            RegisterPermission(Permissions.Create, Modules.CurrencyManager);
            RegisterPermission(Permissions.Activate, Modules.CurrencyManager);
            RegisterPermission(Permissions.Deactivate, Modules.CurrencyManager);

            RegisterPermission(Permissions.Create, Modules.SupportedCurrencies);
            RegisterPermission(Permissions.View, Modules.SupportedCurrencies);

            RegisterPermission(Permissions.Create, Modules.SupportedCountries);
            RegisterPermission(Permissions.View, Modules.SupportedCountries);

            RegisterPermission(Permissions.Create, Modules.SupportedLanguages);
            //AFTREGO-4690
            //RegisterPermission(Permissions.View, Modules.ExchangeRateManager);
            //RegisterPermission(Permissions.Update, Modules.ExchangeRateManager);
            //RegisterPermission(Permissions.Create, Modules.ExchangeRateManager);

            RegisterPermission(Permissions.Create, Modules.LanguageManager);

            RegisterPermission(Permissions.Search, Modules.PlayerManager);
            RegisterPermission(Permissions.AssignPaymentLevel, Modules.PlayerManager);

            RegisterPermission(Permissions.Reject, Modules.OfflineWithdrawalVerification);

            RegisterPermission(Permissions.View, Modules.PaymentGatewaySettings);
            RegisterPermission(Permissions.Update, Modules.PaymentGatewaySettings);
            RegisterPermission(Permissions.Create, Modules.PaymentGatewaySettings);
            RegisterPermission(Permissions.Activate, Modules.PaymentGatewaySettings);
            RegisterPermission(Permissions.Deactivate, Modules.PaymentGatewaySettings);

            RegisterPermission(Permissions.Create, Modules.BonusManager);
            RegisterPermission(Permissions.Update, Modules.BonusManager);
            RegisterPermission(Permissions.Activate, Modules.BonusManager);
            RegisterPermission(Permissions.Deactivate, Modules.BonusManager);
            RegisterPermission(Permissions.View, Modules.BonusManager);
            RegisterPermission(Permissions.Create, Modules.BonusTemplateManager);
            RegisterPermission(Permissions.Update, Modules.BonusTemplateManager);
            RegisterPermission(Permissions.Delete, Modules.BonusTemplateManager);
            RegisterPermission(Permissions.View, Modules.BonusTemplateManager);

            #region Banks

            RegisterPermission(Permissions.ViewEmail, Modules.PlayerReport);
            RegisterPermission(Permissions.ViewMobile, Modules.PlayerReport);

            #endregion


            #region Banks
            RegisterPermission(Permissions.View, Modules.Banks);
            RegisterPermission(Permissions.Create, Modules.Banks);
            RegisterPermission(Permissions.Update, Modules.Banks);
            #endregion

            #region BankAccounts
            RegisterPermission(Permissions.View, Modules.BankAccounts);
            RegisterPermission(Permissions.Create, Modules.BankAccounts);
            RegisterPermission(Permissions.Update, Modules.BankAccounts);
            RegisterPermission(Permissions.Activate, Modules.BankAccounts);
            RegisterPermission(Permissions.Deactivate, Modules.BankAccounts);
            #endregion

            #region PlayerBankAccount
            RegisterPermission(Permissions.View, Modules.PlayerBankAccount);
            RegisterPermission(Permissions.Create, Modules.PlayerBankAccount);
            RegisterPermission(Permissions.Update, Modules.PlayerBankAccount);
            RegisterPermission(Permissions.Verify, Modules.PlayerBankAccount);
            RegisterPermission(Permissions.Reject, Modules.PlayerBankAccount);
            #endregion

            #region Deposit
            RegisterPermission(Permissions.Create, Modules.OfflineDepositRequests);

            RegisterPermission(Permissions.View, Modules.OfflineDepositConfirmation);
            RegisterPermission(Permissions.Confirm, Modules.OfflineDepositConfirmation);

            RegisterPermission(Permissions.Verify, Modules.DepositVerification);
            RegisterPermission(Permissions.Reject, Modules.DepositVerification);
            RegisterPermission(Permissions.View, Modules.DepositVerification);
            RegisterPermission(Permissions.Unverify, Modules.DepositVerification);

            RegisterPermission(Permissions.Approve, Modules.DepositApproval);
            RegisterPermission(Permissions.Reject, Modules.DepositApproval);
            RegisterPermission(Permissions.View, Modules.DepositApproval);
            #endregion

            #region Player-PaymentLevelSettings
            RegisterPermission(Permissions.View, Modules.PaymentLevelSettings);
            RegisterPermission(Permissions.Update, Modules.PaymentLevelSettings);
            #endregion
        }

        private void RegisterPermission(string name, string module)
        {
            if (_authRepository.Permissions.Any(p => p.Name == name && p.Module == module))
                return;

            _authCommands.CreatePermission(new CreatePermission
            {
                Module = module,
                Name = name
            });
        }

        private void AddPaymentPermissions(List<Permission> permissions,
            bool enablePleyerDepositVerify = true,
            bool enablePlayerDepositApprove = true,
            bool enablePaymentGatewaySettings = true,
            bool enablePaymentLevelManager = true,
            bool enablePaymentSetting = true,
            bool enableTransferSetting = true
            )
        {

            #region PlayerDepositVerify
            if (enablePleyerDepositVerify)
            {
                permissions.AddRange(new List<Permission>
                {
                    new Permission {Name = Permissions.View, Module = Modules.DepositVerification},
                    new Permission {Name = Permissions.Reject, Module = Modules.DepositVerification},
                    new Permission {Name = Permissions.Verify, Module = Modules.DepositVerification},
                    new Permission {Name = Permissions.Unverify, Module = Modules.DepositVerification}
                });
            }

            #endregion 

            #region PlayerDepositApprove
            if (enablePlayerDepositApprove)
            {
                permissions.AddRange(new List<Permission>
                {
                    new Permission {Name = Permissions.View, Module = Modules.DepositApproval},
                    new Permission {Name = Permissions.Reject, Module = Modules.DepositApproval},
                    new Permission {Name = Permissions.Approve, Module = Modules.DepositApproval},
                });
            }

            #endregion

            #region PaymentGatewaySettings

            if (enablePaymentGatewaySettings)
            {
                permissions.AddRange(new List<Permission>
                {
                    new Permission {Name = Permissions.View, Module = Modules.PaymentGatewaySettings},
                    new Permission {Name = Permissions.Create, Module = Modules.PaymentGatewaySettings},
                    new Permission {Name = Permissions.Update, Module = Modules.PaymentGatewaySettings},
                    new Permission {Name = Permissions.Activate, Module = Modules.PaymentGatewaySettings},
                    new Permission {Name = Permissions.Deactivate, Module = Modules.PaymentGatewaySettings}
                });
            }

            #endregion 

            #region PaymentLevelManager
            if (enablePaymentLevelManager)
            {
                permissions.AddRange(new List<Permission>
                {
                    new Permission {Name = Permissions.View, Module = Modules.PaymentLevelManager},
                    new Permission {Name = Permissions.Create, Module = Modules.PaymentLevelManager},
                    new Permission {Name = Permissions.Update, Module = Modules.PaymentLevelManager},
                    new Permission {Name = Permissions.Activate, Module = Modules.PaymentLevelManager},
                    new Permission {Name = Permissions.Deactivate, Module = Modules.PaymentLevelManager}
                });
            }
            #endregion 

            #region PaymentSettings
            if (enablePaymentSetting)
            {
                permissions.AddRange(new List<Permission>
                {
                    new Permission {Name = Permissions.View, Module = Modules.PaymentSettings},
                    new Permission {Name = Permissions.Create, Module = Modules.PaymentSettings},
                    new Permission {Name = Permissions.Update, Module = Modules.PaymentSettings},
                    new Permission {Name = Permissions.Activate, Module = Modules.PaymentSettings},
                    new Permission {Name = Permissions.Deactivate, Module = Modules.PaymentSettings}
                });
            }
            #endregion

            #region TransferSettings
            if (enableTransferSetting)
            {
                permissions.AddRange(new List<Permission>
                {
                    new Permission {Name = Permissions.View, Module = Modules.TransferSettings},
                    new Permission {Name = Permissions.Create, Module = Modules.TransferSettings},
                    new Permission {Name = Permissions.Update, Module = Modules.TransferSettings},
                    new Permission {Name = Permissions.Activate, Module = Modules.TransferSettings},
                    new Permission {Name = Permissions.Deactivate, Module = Modules.TransferSettings}
                });
            }
            #endregion 
        }
    }
}
