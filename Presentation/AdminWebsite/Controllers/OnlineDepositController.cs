using System;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class OnlineDepositController : BaseController
    {              
        [HttpGet]
        public ActionResult Get(Guid id)
        {
            var response = GetAdminApiProxy(Request).GetOnlineDepositById(id);
            return this.Success(response);
        }

        [HttpPost]
        public ActionResult Verify(Guid id, string remarks)
        {
            var response = GetAdminApiProxy(Request).VerifyOnlineDeposit(
                    new AdminApi.Interface.Payment.VerifyOnlineDepositRequest
                    {
                        Id = id,
                        Remarks = remarks
                    });

            return response.Success ? this.Success() : this.Failed(response.Errors);
        }

        [HttpPost]
        public ActionResult Unverify(Guid id, string remarks)
        {
            var response = GetAdminApiProxy(Request).UnverifyOnlineDeposit(
                    new AdminApi.Interface.Payment.UnverifyOnlineDepositRequest
                    {
                        Id = id,
                        Remarks = remarks
                    });

            return response.Success ? this.Success() : this.Failed(response.Errors);
        }

        [HttpPost]
        public ActionResult Reject(Guid id, string remarks)
        {
            var response = GetAdminApiProxy(Request).RejectOnlineDeposit(
                    new AdminApi.Interface.Payment.RejectOnlineDepositRequest
                    {
                        Id = id,
                        Remarks = remarks
                    });

            return response.Success ? this.Success() : this.Failed(response.Errors);
        }

        [HttpPost]
        public ActionResult Approve(Guid id, string remarks)
        {
            var response = GetAdminApiProxy(Request).ApproveOnlineDeposit(
                    new AdminApi.Interface.Payment.ApproveOnlineDepositRequest
                    {
                        Id = id,
                        Remarks = remarks
                    });

            return response.Success ? this.Success() : this.Failed(response.Errors);
        }      
    }
}