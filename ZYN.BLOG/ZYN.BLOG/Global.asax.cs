using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using ZYN.BLOG.SiteSearch;
using ZYN.BLOG.WebHelper;

namespace ZYN.BLOG
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure(
               new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\log4net.config")
           );

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //初始化七牛Key
            QiniuHelper.SetKey(); //之后不用再初始化

            //启动Lucene索引处理任务
            InvertedIndex.IndexManager.Start();

            //执行搜索热词统计任务
            QuartzTick.StartJob(); //10分钟执行一次
        }
    }
}