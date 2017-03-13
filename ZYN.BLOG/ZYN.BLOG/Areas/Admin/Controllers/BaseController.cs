using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ZYN.BLOG.Model;
using ZYN.BLOG.ViewModel;

namespace ZYN.BLOG.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller本身继承了各种FilterAttribute（IActionFilter、IResultFilter等）
    /// </summary>
    public class BaseController : Controller
    {
        IBLL.IWebSettingService webSetService = WebHelper.OperateHelper.Current.serviceSession.WebSettingService;

        public LoginUser loginuser { get; set; }

        /// <summary>
        /// 登录校验
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            HttpCookie cookie = Request.Cookies["ZynBlogTicket"];

            string name = string.Empty;

            if (cookie != null)
            {
                string ticketString = cookie.Value;

                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(ticketString);

                name = ticket.Name;

                if (!string.IsNullOrEmpty(name))
                {
                    ViewBag.AdminUser = name;

                    return;
                }
                else
                {
                    //跳转到登录页
                    filterContext.HttpContext.Response.RedirectToRoute("Admin_default", new { controller = "Account", action = "Index" });
                    return;
                }
            }
            else
            {
                //跳转到登录页
                filterContext.HttpContext.Response.RedirectToRoute("Admin_default", new { controller = "Account", action = "Index" });
                return;
            }




            ////用Seesion判断登录状态:如果Session['loginuser']不为空(用户在线状态),则继续。
            //loginuser = Session["loginuser"] as LoginUser;
            //if (loginuser != null)
            //{
            //    ViewBag.AdminUser = loginuser.UserName;

            //    return;
            //}
            //else
            //{
            //    //跳转到登录页
            //    filterContext.HttpContext.Response.RedirectToRoute("Admin_default", new { controller = "Account", action = "Index" });
            //    return;
            //}
        }
    }
}
