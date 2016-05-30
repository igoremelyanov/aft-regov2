using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data.Brand.ContentTranslations.Base;

namespace AFT.RegoV2.Core.Common.Data.Brand.ContentTranslations
{
    public class AddContentTranslationModel : ContentTranslationBase
    {
        public IList<string> Languages { get; set; }
        public IList<AddContentTranslationData> Translations { get; set; }
    }
}