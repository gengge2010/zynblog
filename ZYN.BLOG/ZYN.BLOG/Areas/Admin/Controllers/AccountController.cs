using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ZYN.BLOG.Common;
using ZYN.BLOG.Model;
using ZYN.BLOG.ViewModel;

namespace ZYN.BLOG.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        IBLL.IWebSettingService webSetService = WebHelper.OperateHelper.Current.serviceSession.WebSettingService;

        public ActionResult Index()
        {
            return View();
        }

        #region 1.0 登录 记录Session
        /// <summary>
        /// 1.0 后台登录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(LoginUser userModel)
        {
            //实体验证成功的话 进一步验证
            if (ModelState.IsValid)
            {
                WebSetting nameSetting = webSetService.GetDataListBy(w => w.ConfigKey == "AdminName").FirstOrDefault();
                WebSetting pwdSetting = webSetService.GetDataListBy(w => w.ConfigKey == "AdminSecret").FirstOrDefault();

                //校验成功,将用户信息保存到Session中，并将票据写入cookie,跳转至后台首页
                //后台中的每个Controller都要继承一个BaseController,BaseController中要先校验用户有没有登录,
                //之后才能进行Action操作
                //该用户校验通过：写完cookie和sesion后跳转到首页

                string Md5Pwd = userModel.PassWord;
                if (nameSetting.ConfigValue == userModel.UserName && pwdSetting.ConfigValue == Common.Security.StrToMD5(userModel.PassWord))
                {
                    ////写Session //跳转到首页 
                    //Session.Add("loginuser", userModel);

                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                        2,
                        userModel.UserName, 
                        DateTime.Now,
                        DateTime.Now.AddDays(30),
                        true,
                        string.Empty
                        );
                    HttpCookie cookie = new HttpCookie("ZynBlogTicket");
                    string ticketString = FormsAuthentication.Encrypt(ticket);
                    cookie.Value = ticketString;
                    cookie.Expires = DateTime.Now.AddDays(30);  //cookie的过期时间
                    this.Response.Cookies.Add(cookie);

                    return Json(new
                    {
                        Status = 1,
                        CoreData = "/Admin/AdminHome/Index"
                    });
                }
                else
                {
                    return Json(new
                    {
                        Status = 0,
                        Message = "用户名或密码错误"
                    });
                }
            }
            else
            {
                return Json(new
                {
                    Status = 0,
                    Message = "没通过验证,请核对信息"
                });
            }
        }
        #endregion

        #region 2.0 退出登录、清除Session
        /// <summary>
        /// 退出登录：删除session,跳转到登录页
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            ////删除Session
            //if (Session["loginuser"] != null)
            //    Session.Remove("loginuser");

            HttpCookie cookie = new HttpCookie("ZynBlogTicket");
            cookie.Expires = new DateTime(1990 - 11 - 23);
            this.Response.Cookies.Add(cookie);

            //跳转到登录页
            return RedirectToRoute("Admin_default", new { controller = "Account", action = "Index" });
        }
        #endregion
    }
}
