using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZYN.BLOG.jqDataTableModel;
using ZYN.BLOG.Model;

namespace ZYN.BLOG.Areas.Admin.Controllers
{
    public class AdminCommentController : BaseController
    {
        IBLL.ICommentService commentService = WebHelper.OperateHelper.Current.serviceSession.CommentService;

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 2.0 jquery.datatable()插件 Ajax Get评论列表
        ///     按文章编号来排序吧
        /// </summary>
        /// <returns>datatable-JSON</returns>
        public JsonResult GetCommentsJson(jqDataTableParameter tableParam)
        {
            //0.0 全部数据   先按文章编号排序，再按时间排序
            List<Comment> DataSource = commentService.GetDataListBy(c => true, c => c.CmtArtId);
            //DataSource = DataSource.OrderBy(c => c.SubTime).ToList();


            //1.0 首先获取datatable提交过来的参数
            string echo = tableParam.sEcho;  //用于客户端自己的校验
            int dataStart = tableParam.iDisplayStart;//要请求的该页第一条数据的序号
            int pageSize = tableParam.iDisplayLength == -1 ? DataSource.Count : tableParam.iDisplayLength;//每页容量（=-1表示取全部数据）
            string search = tableParam.sSearch;

            //2.0 根据参数(起始序号、每页容量、查询参数)查询数据
            if (!String.IsNullOrEmpty(search))
            {
                var data = DataSource.Where(c => c.CmtText.Contains(search))
                     .Skip<Comment>(dataStart)
                     .Take(pageSize)
                     .Select(c => new
                     {
                         Id = c.Id,
                         ArticleId = c.CmtArtId,
                         CmtText = c.CmtText,
                         CmterName = c.Visitor.VisitorName,
                         CmterEmail = c.Visitor.VisitorEmail,
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
            else
            {
                var data = DataSource.Skip<Comment>(dataStart)
                                     .Take(pageSize)
                                     .Select(c => new
                                     {
                                         Id = c.Id,
                                         ArticleId = c.CmtArtId,
                                         CmtText = c.CmtText,
                                         CmterName = c.Visitor.VisitorName,
                                         CmterEmail = c.Visitor.VisitorEmail,
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
        }

        /// <summary>
        /// 3.0 根据id(软删除)分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteComment(int id)
        {
            Comment comment = new Comment()
            {
                Id = id,
                Status = 0
            };

            int val = commentService.Update(comment, "Status");

            return Json(val);
        }


        /// <summary>
        /// 4.0 根据id将分类设为启用状态
        /// </summary>
        [HttpPost]
        public ActionResult PublicComment(int id)
        {
            Comment comment = new Comment()
            {
                Id = id,
                Status = 1
            };

            int val = commentService.Update(comment, "Status");

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
            Comment model = new Comment()
            {
                Id = Id
            };
            model.CmtText = editedContent;
            model.AltTime = DateTime.Now;

            int val = commentService.Update(model, "CmtText", "AltTime");

            return Json(val);
        }

        /// <summary>
        /// 回复评论
        /// </summary>
        /// <param name="Id">被回复的评论的Id</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ReplyComment(int Id)
        {
            string artId = Request["artId"];
            string editedContent = Request["replyinput"];

            Comment model = new Comment()
            {
                CmtArtId = Convert.ToInt32(artId),
                ParentId = Id,
                VisitorId = 197,  //博主我本身
                CmtText = editedContent,
                Status = 1,
                SubTime = DateTime.Now
            };

            int val = commentService.Add(model);

            return Json(val);
        }

    }
}
