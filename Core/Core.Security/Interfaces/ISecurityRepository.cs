using System;
using System.Data.Entity;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Interface.Data;

namespace AFT.RegoV2.Core.Security.Interfaces
{
    public interface ISecurityRepository
    {
        IDbSet<Role> Roles { get; }
        IDbSet<Admin> Admins { get; }
        IDbSet<AdminIpRegulation> AdminIpRegulations { get; }
        IDbSet<BrandIpRegulation> BrandIpRegulations { get; }
        IDbSet<AdminIpRegulationSetting> AdminIpRegulationSettings { get; }
        IDbSet<Error> Errors { get; }
        IDbSet<Data.Brand> Brands { get; }
        Admin GetAdminById(Guid userId);
        int SaveChanges();
    }
}