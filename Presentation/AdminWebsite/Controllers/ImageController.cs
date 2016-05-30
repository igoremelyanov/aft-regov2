using System;
using System.Web.Mvc;
using AFT.RegoV2.Core.Common;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class ImageController : BaseController
    {
        private readonly IDocumentService _documentService;

        public ImageController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public ActionResult Show(Guid fileId, Guid playerId)
        {
            var imageData = _documentService.GetFile(fileId, playerId);
            return File(imageData, "image/jpg");
        }
    }
}