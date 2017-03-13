using System.Web;
using System.Web.Mvc;
using ZYN.BLOG.Filter;

namespace ZYN.BLOG
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());

            filters.Add(new StatisticsTrackerAttribute());
        }
    }
}