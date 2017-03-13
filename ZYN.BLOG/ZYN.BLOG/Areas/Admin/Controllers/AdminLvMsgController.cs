using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZYN.BLOG.jqDataTableModel;
using ZYN.BLOG.Model;

namespace ZYN.BLOG.Areas.Admin.Controllers
{
    public class AdminLvMsgController : Controller
    {
        IBLL.ILeaveMsgService lvmService = WebHelper.OperateHelper.Current.serviceSession.LeaveMsgService;

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 2.0 jquery.datatable()插件 Ajax Get评论列表
        ///     按文章编号来排序吧
        /// </summary>
        /// <returns>datatable-JSON</returns>
        public JsonResult GetLvMsgsJson(jqDataTableParameter tableParam)
        {
            //0.0 全部数据 按什么排序呢？
            List<LeaveMsg> DataSource = lvmService.GetDataListBy(c => true);

            //1.0 首先获取datatable提交过来的参数
            string echo = tableParam.sEcho;  //用于客户端自己的校验
            int dataStart = tableParam.iDisplayStart;//要请求的该页第一条数据的序号
            int pageSize = tableParam.iDisplayLength == -1 ? DataSource.Count : tableParam.iDisplayLength;

            //2.0 根据参数(起始序号、每页容量、查询参数)查询数据
            var data = DataSource.Skip<LeaveMsg>(dataStart)
                                 .Take(pageSize)
                                 .Select(c => new
                                 {
                                     Id = c.Id,
                                     LvmText = c.LMessage,
                                     LvmerName = c.Visitor.VisitorName,
                                     LvmerEmail = c.Visitor.VisitorEmail,
                                     SubTime = c.SubTime.ToString(),
                                     Status = c.Status
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
        public ActionResult DeleteLeaveMsg(int id)
        {
            LeaveMsg comment = new LeaveMsg()
            {
                Id = id,
                Status = 0
            };

            int val = lvmService.Update(comment, "Status");

            return Json(val);
        }


        /// <summary>
        /// 4.0 根据id将分类设为启用状态
        /// </summary>
        [HttpPost]
        public ActionResult PublicLeaveMsg(int id)
        {
            LeaveMsg comment = new LeaveMsg()
            {
                Id = id,
                Status = 1
            };

            int val = lvmService.Update(comment, "Status");

            return Json(val);
        }

        /// <summary>
        /// 5.2 ajax post 更新被修改的model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetEditedEntity(int Id)
        {
            string editedContent = Request["editinput"];
            LeaveMsg model = new LeaveMsg()
            {
                Id = Id
            };
            model.LMessage = editedContent;
            model.AltTime = DateTime.Now;

            int val = lvmService.Update(model, "LMessage", "AltTime");

            return Json(val);
        }

        /// <summary>
        /// 回复评论
        /// </summary>
        /// <param name="Id">被回复的评论的Id</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ReplyLeaveMsg(int Id)
        {
            string editedContent = Request["replyinput"];

            LeaveMsg model = new LeaveMsg()
            {
                ParentId = Id,
                VisitorId = 197,  //博主我本身
                LMessage = editedContent,
                Status = 1,
                SubTime = DateTime.Now
            };

            int val = lvmService.Add(model);

            return Json(val);
        }

    }
}
