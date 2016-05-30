using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Filters;

namespace AFT.RegoV2.AdminWebsite
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new RequreSecureConnectionFilter());
        }
    }
}