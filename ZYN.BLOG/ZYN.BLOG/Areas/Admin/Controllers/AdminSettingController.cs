using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZYN.BLOG.jqDataTableModel;
using ZYN.BLOG.Model;

namespace ZYN.BLOG.Areas.Admin.Controllers
{
    public class AdminSettingController : Controller
    {
        IBLL.IWebSettingService settingService = WebHelper.OperateHelper.Current.serviceSession.WebSettingService;
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 2.0 jquery.datatable()插件 Ajax Get设置列表
        /// </summary>
        /// <returns>datatable-JSON</returns>
        public JsonResult GetSettingsJson(jqDataTableParameter tableParam)
        {
            List<WebSetting> DataSource = settingService.GetDataListBy(a => true, a => a.Id);

            string echo = tableParam.sEcho;
            int dataStart = tableParam.iDisplayStart;
            int pageSize = tableParam.iDisplayLength == -1 ? DataSource.Count : tableParam.iDisplayLength;

            var data = DataSource.Skip<WebSetting>(dataStart)
                                 .Take(pageSize)
                                 .Select(s => new
                                 {
                                     Id = s.Id,
                                     ConfigKey = s.ConfigKey,
                                     ConfigValue = s.ConfigValue,
                                     Description = s.Description,
                                     BuildTime = s.BuildTime.ToString()
                                 }).ToList();

            return Json(new
            {
                sEcho = echo,
                iTotalRecords = DataSource.Count(),
                iTotalDisplayRecords = DataSource.Count(),
                aaData = data
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 5.1 ajax get 获取被修改的model
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetEditedEntity(int id)
        {
            WebSetting entity = settingService.GetEntity(id);

            return Json(new
            {
                Id = entity.Id,
                ConfigKey = entity.ConfigKey,
                ConfigValue = entity.ConfigValue,
                Description = entity.Description,
                BuildTime = entity.BuildTime.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 5.2 ajax post 更新被修改的model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetEditedEntity(WebSetting model)
        {
            model.AltTime = DateTime.Now;
            int val = settingService.Update(model, "ConfigKey", "ConfigValue", "Description", "BuildTime", "AltTime");

            return Json(val);
        }

        /// <summary>
        /// 6.0 新增设置
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddSetting(WebSetting model)
        {
            model.BuildTime = DateTime.Now;

            int val = settingService.Add(model);

            return Json(val);
        }
    }
}
