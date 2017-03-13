using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ZYN.BLOG.Areas.Admin.Controllers
{
    public class AdminHomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

    }
}
