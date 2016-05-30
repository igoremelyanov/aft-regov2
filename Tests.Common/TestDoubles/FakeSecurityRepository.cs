using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Interface.Data;
using AFT.RegoV2.Core.Security.Interfaces;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeSecurityRepository : ISecurityRepository
    {
        private readonly FakeDbSet<Admin> _admins = new FakeDbSet<Admin>();
        private readonly FakeDbSet<Role> _roles = new FakeDbSet<Role>();
        private readonly FakeDbSet<AdminIpRegulation> _adminIpRegulations = new FakeDbSet<AdminIpRegulation>();
        private readonly FakeDbSet<BrandIpRegulation> _brandIpRegulations = new FakeDbSet<BrandIpRegulation>();
        private readonly FakeDbSet<AdminIpRegulationSetting> _adminIpRegulationSettings = new FakeDbSet<AdminIpRegulationSetting>();
        private readonly FakeDbSet<Error> _errors = new FakeDbSet<Error>();
        private readonly FakeDbSet<Brand> _brands = new FakeDbSet<Brand>();

        public  IDbSet<Admin> Admins
        {
            get { return _admins; }
        }

        public  IDbSet<Role> Roles
        {
            get { return _roles; }
        }

        public IDbSet<AdminIpRegulation> AdminIpRegulations
        {
            get { return _adminIpRegulations; }
        }

        public IDbSet<AdminIpRegulationSetting> AdminIpRegulationSettings
        {
            get { return _adminIpRegulationSettings; }
        }

        public IDbSet<BrandIpRegulation> BrandIpRegulations
        {
            get { return _brandIpRegulations; }
        }

        public IDbSet<Error> Errors
        {
            get { return _errors; }
        }

        public IDbSet<Brand> Brands
        {
            get { return _brands; }
        }

        public Admin GetAdminById(Guid userId)
        {
            return _admins.Single(u => u.Id == userId);
        }

        public int SaveChanges()
        {
            return 1;
        }
    }
}
