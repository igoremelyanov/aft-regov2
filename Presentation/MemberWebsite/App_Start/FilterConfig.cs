using System.Web.Mvc;
using AFT.RegoV2.MemberWebsite.Common;

namespace AFT.RegoV2.MemberWebsite
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // we are using Elmah filter
            //filters.Add(new HandleErrorAttribute());
        }
    }
}