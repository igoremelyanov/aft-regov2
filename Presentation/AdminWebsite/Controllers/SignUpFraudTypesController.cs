using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Fraud.Validations;
using AFT.RegoV2.Core.Security.Interfaces;
using ServiceStack.Common.Extensions;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class SignUpFraudTypesController : BaseController
    {
        private readonly SignUpFraudTypeCommands _commands;
        private readonly SignUpFraudTypeQueries _queries;
        private readonly SignUpFraudTypeValidator _validator;
        private readonly ISecurityRepository _securityRepository;

        public SignUpFraudTypesController(
            SignUpFraudTypeCommands commands,
            SignUpFraudTypeQueries queries,
            SignUpFraudTypeValidator validator,
            ISecurityRepository securityRepository)
        {
            _commands = commands;
            _queries = queries;
            _validator = validator;
            _securityRepository = securityRepository;
        }


        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<SignUpFraudType>(
                searchPackage,
                _queries.GetSignUpFraudTypes().AsQueryable());

            dataBuilder
                .Map(configuration => configuration.Id,
                    obj => new[]
                    {
                        obj.Name,
                        GetDescription(obj.SystemAction.ToString()),
                        string.Join("\r\n", obj.RiskLevels.Select(o=>o.Name)),
                        obj.Remarks,
                        obj.CreatedBy,
                        Format.FormatDate(obj.DateCreated, true)
                    });
            var data = dataBuilder.GetPageData(configuration => configuration.Name);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private string GetDescription(string name)
        {
            var type = typeof(SystemAction);
            var memInfo = type.GetMember(name);
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Any())
                return ((DescriptionAttribute)attributes[0]).Description;

            return "-";
        }

        [HttpGet]
        public object GetSystemActions()
        {
            var values = Enum.GetValues(typeof(SystemAction));

            return SerializeJson(new
            {
                SystemActions = values.Cast<int>().Select(o => new
                {
                    Value = o,
                    Name = GetDescription(Enum.GetName(typeof(SystemAction), o))
                })
            });
        }

        [HttpGet]
        public string GetFraudRiskLevels()
        {
            var user = _securityRepository.Admins
                .Include(u => u.BrandFilterSelections)
                .Single(u => u.Id == CurrentUser.Id);

            var riskLevels = _queries.GetActiveRiskLevels(user.BrandFilterSelections.Select(o => o.BrandId));
            return SerializeJson(new
            {
                RiskLevels = riskLevels.Select(x => new { id = x.Id, name = x.Name })
            });
        }

        [HttpPost]
        public ActionResult AddOrUpdate(SignUpFraudTypeDTO data)
        {
            var result = _validator.Validate(data);
            if (!result.IsValid)
                return this.Failed(new
                {
                    code = result.Errors[0].ErrorMessage
                });

            try
            {
                if (data.Id == Guid.Empty)
                    _commands.Create(data, CurrentUser.UserName);
                else
                    _commands.Update(data, CurrentUser.UserName);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
            return this.Success(new
            {
                code = data.Id == Guid.Empty
                    ? "successfullyCreated"
                    : "successfullyUpdated"
            });
        }

        [HttpGet]
        public string GetById(Guid id)
        {
            var entity = _queries.GetById(id);

            return SerializeJson(new
            {
                Id = entity.Id,
                FraudTypeName = entity.Name,
                Remarks = entity.Remarks,
                RiskLevels = entity.RiskLevels.Select(o => o.Id).ToArray(),
                SystemAction = entity.SystemAction
            });
        }
    }
}