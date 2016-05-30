using System;
using AFT.RegoV2.Core.Common.Data.Brand.ContentTranslations.Base;

namespace AFT.RegoV2.Core.Common.Data.Brand.ContentTranslations
{
    public class EditContentTranslationData : ContentTranslationBase
    {
        public Guid Id { get; set; }
        public string Language { get; set; }
        public string Translation { get; set; }
        public string Remark { get; set; }
    }
}
