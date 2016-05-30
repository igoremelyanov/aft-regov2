using AFT.RegoV2.Core.Common.Data.Brand.ContentTranslations.Base;

namespace AFT.RegoV2.Core.Common.Data.Brand.ContentTranslations
{
    public class AddContentTranslationData : ContentTranslationBase
    {
        public string Language { get; set; }
        public string Translation { get; set; }
    }
}
