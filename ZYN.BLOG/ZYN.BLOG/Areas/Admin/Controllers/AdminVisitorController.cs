using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZYN.BLOG.jqDataTableModel;
using ZYN.BLOG.Model;

namespace ZYN.BLOG.Areas.Admin.Controllers
{
    public class AdminVisitorController : BaseController
    {
        IBLL.IVisitorService visitorService = WebHelper.OperateHelper.Current.serviceSession.VisitorService;
        /// <summary>
        /// 1.0 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 2.0 jquery.datatable()插件 Ajax Get游客列表
        /// </summary>
        /// <returns>datatable-JSON</returns>
        public JsonResult GetVisitorsJson(jqDataTableParameter tableParam)
        {
            //0.0 全部数据
            List<Visitor> DataSource = visitorService.GetDataListBy(v => true, v => v.SubTime);

            //1.0 首先获取datatable提交过来的参数
            string echo = tableParam.sEcho;  //用于客户端自己的校验
            int dataStart = tableParam.iDisplayStart;//要请求的该页第一条数据的序号
            int pageSize = tableParam.iDisplayLength == -1 ? DataSource.Count : tableParam.iDisplayLength;//每页容量（=-1表示取全部数据）

            //2.0 根据参数(起始序号、每页容量、查询参数)查询数据
            string defaultIconPath = ConfigurationManager.AppSettings["DefaultHeadIcon"];
            var data = DataSource.Skip<Visitor>(dataStart)
                                 .Take(pageSize)
                                 .Select(v => new
                                 {
                                     Id = v.Id,
                                     HeadImg = v.HeadIcon == null ? defaultIconPath : v.HeadIcon.IconURL,
                                     Name = v.VisitorName,
                                     Email = v.VisitorEmail,
                                     QQ = v.VisitorQQ,
                                     SubTime = v.SubTime.ToString(),
                                     Status = v.Status
                                 }).ToList();

            //3.0 构造datatable所需要的数据json对象...aaData里面应是一个二维数组：即里面是一个数组[["","",""],[],[],[]]
            return Json(new
            {
                sEcho = echo,
                iTotalRecords = DataSource.Count(),
                iTotalDisplayRecords = DataSource.Count(),
                aaData = data
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 3.0 根据id(软删除)分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteVisitor(int id)
        {
            Visitor visitor = new Visitor()
            {
                Id = id,
                Status = 0
            };

            int val = visitorService.Update(visitor, "Status");

            return Json(val);
        }

        /// <summary>
        /// 4.0 根据id将分类设为启用状态
        /// </summary>
        [HttpPost]
        public ActionResult PublicVisitor(int id)
        {
            Visitor visitor = new Visitor()
            {
                Id = id,
                Status = 1
            };

            int val = visitorService.Update(visitor, "Status");

            return Json(val);
        }

        /// <summary>
        /// 5.1 ajax get 获取被修改的model
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetEditedEntity(int id)
        {
            Visitor entity = visitorService.GetEntity(id);

            return Json(new
            {
                Id = entity.Id,
                Name = entity.VisitorName,
                HeadImgId = entity.VisitorIconId,
                Email = entity.VisitorEmail,
                QQ = string.IsNullOrEmpty(entity.VisitorQQ) ? "" : entity.VisitorQQ,
                Status = entity.Status,
                SubTime = entity.SubTime.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 5.2 ajax post 更新被修改的model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetEditedEntity(Visitor model)
        {
            model.AltTime = DateTime.Now;
            int val = visitorService.Update(model, "VisitorName", "VisitorEmail", "VisitorQQ", "Status", "SubTime", "AltTime");

            return Json(val);
        }

    }
}
