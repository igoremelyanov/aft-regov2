using System;
using System.Linq;
using System.Text;
using AFT.RegoV2.AdminWebsite.Controllers;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.WinService.Workers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Report.Brand
{
    internal class BrandReportTest : ReportsTestsBase
    {
        private IReportRepository _reportRepository;
        private ReportQueries _reportQueries;
        private BrandCommands _brandCommands;
        private BrandQueries _brandQueries;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _reportRepository = Container.Resolve<IReportRepository>();
            _reportQueries = Container.Resolve<ReportQueries>();
            _brandCommands = Container.Resolve<BrandCommands>();
            _brandQueries = Container.Resolve<BrandQueries>();
        }

        protected override void StartWorkers()
        {
            Container.Resolve<BrandReportWorker>().Start();
        }

        [Test]
        public void Can_process_brand_registered()
        {
            // Act
            var brand = BrandTestHelper.CreateBrand();

            // Assert
            Assert.AreEqual(2, _reportRepository.BrandRecords.Count());
            var record = _reportRepository.BrandRecords.Last();
            Assert.AreEqual(brand.Id, record.BrandId);
            Assert.AreEqual(brand.Licensee.Name, record.Licensee);
            Assert.AreEqual(brand.Code, record.BrandCode);
            Assert.AreEqual(brand.Name, record.Brand);
            Assert.AreEqual(brand.Type.ToString(), record.BrandType);
            Assert.AreEqual(brand.PlayerPrefix, record.PlayerPrefix);
            Assert.AreEqual(brand.InternalAccountsNumber, record.AllowedInternalAccountsNumber);
            Assert.AreEqual(brand.Status.ToString(), record.BrandStatus);
            Assert.AreEqual(TimeZoneInfo.GetSystemTimeZones().Single(z => z.Id == brand.TimezoneId).DisplayName,
                record.BrandTimeZone);
            record.Created.Should().BeCloseTo(brand.DateCreated, 50);
            Assert.AreEqual(brand.CreatedBy, record.CreatedBy);
        }

        [Test]
        public void Can_process_brand_updated()
        {
            // Arrange
            var brand = BrandTestHelper.CreateBrand();
            var newBrandName = TestDataGenerator.GetRandomString(20);
            var newBrandCode = TestDataGenerator.GetRandomString(20);
            var newTimeZoneId = TestDataGenerator.GetRandomTimeZone().Id;
            var newInternalAccountCount = TestDataGenerator.GetRandomNumber(10, 0);
            const string remarks = "Test updating brand";

            string newPlayerPrefix;

            do
            {
                newPlayerPrefix = TestDataGenerator.GetRandomString(3);
            } 
            while (_brandQueries.GetBrands()
                    .Any(x => x.PlayerPrefix == newPlayerPrefix && x.Licensee.Id == brand.Licensee.Id));

            // Act
            _brandCommands.EditBrand(new EditBrandRequest
            {
                Brand = brand.Id,
                Code = newBrandCode,
                Email = brand.Email,
                WebsiteUrl = brand.WebsiteUrl,
                EnablePlayerPrefix = true,
                InternalAccounts = newInternalAccountCount,
                Licensee = brand.Licensee.Id,
                SmsNumber = TestDataGenerator.GetRandomPhoneNumber().Replace("-", ""),
                Name = newBrandName,
                PlayerPrefix = newPlayerPrefix,
                TimeZoneId = newTimeZoneId,
                Type = brand.Type,
                Remarks = remarks
            });

            // Assert
            Assert.AreEqual(2, _reportRepository.BrandRecords.Count());
            var record = _reportRepository.BrandRecords.Last();
            Assert.AreEqual(brand.Id, record.BrandId);
            Assert.AreEqual(brand.Licensee.Name, record.Licensee);
            Assert.AreEqual(newBrandCode, record.BrandCode);
            Assert.AreEqual(newBrandName, record.Brand);
            Assert.AreEqual(newPlayerPrefix, record.PlayerPrefix);
            Assert.AreEqual(newInternalAccountCount, record.AllowedInternalAccountsNumber);
            Assert.AreEqual(TimeZoneInfo.GetSystemTimeZones().Single(z => z.Id == newTimeZoneId).DisplayName,
                record.BrandTimeZone);
            record.Updated.Should().BeCloseTo(brand.DateUpdated.Value);
            Assert.AreEqual(brand.UpdatedBy, record.UpdatedBy);
            Assert.AreEqual(remarks, record.Remarks);
        }

        [Test]
        public void Can_process_brand_activated()
        {
            // Arrange
            var brand = BrandTestHelper.CreateBrand();

            // Act
            _brandCommands.ActivateBrand(new ActivateBrandRequest { BrandId = brand.Id, Remarks = "Test activating brand" });

            // Assert
            Assert.AreEqual(2, _reportRepository.BrandRecords.Count());
            var record = _reportRepository.BrandRecords.Last();
            Assert.AreEqual(brand.Id, record.BrandId);
            Assert.AreEqual(brand.Status.ToString(), record.BrandStatus);
            record.Activated.Value.Should().BeCloseTo(brand.DateActivated.Value, 50);
            Assert.AreEqual(brand.ActivatedBy, record.ActivatedBy);
        }

        [Test]
        public void Can_process_brand_deactivated()
        {
            // Arrange
            var brand = BrandTestHelper.CreateBrand();
            _brandCommands.ActivateBrand(new ActivateBrandRequest { BrandId = brand.Id, Remarks = "Test activating brand" });

            // Act
            _brandCommands.DeactivateBrand(new DeactivateBrandRequest { BrandId = brand.Id, Remarks = "Test deactivating brand" });

            // Assert
            Assert.AreEqual(2, _reportRepository.BrandRecords.Count());
            var record = _reportRepository.BrandRecords.Last();
            Assert.AreEqual(brand.Id, record.BrandId);
            Assert.AreEqual(brand.Status.ToString(), record.BrandStatus);
            record.Deactivated.Value.Should().BeCloseTo(brand.DateDeactivated.Value, 50);
            Assert.AreEqual(brand.DeactivatedBy, record.DeactivatedBy);
        }


        [Test]
        public void Can_export_report_data()
        {
            // Arrange
            BrandTestHelper.CreateBrand();

            var filteredRecords = ReportController.FilterAndOrder(
                _reportQueries.GetBrandRecordsForExport(),
                new BrandRecord(),
                "Created", "asc");

            // Act
            var content = Encoding.Unicode.GetString(ReportController.ExportToExcel(filteredRecords));

            // Assert
            Assert.AreNotEqual(content.IndexOf("<table"), -1);
        }
    }
}