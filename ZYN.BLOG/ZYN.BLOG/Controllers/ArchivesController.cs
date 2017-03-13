using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using ZYN.BLOG.Common;
using ZYN.BLOG.FormatModel;
using ZYN.BLOG.Model;
using ZYN.BLOG.WebHelper;

namespace ZYN.BLOG.Controllers
{
    public class ArchivesController : Controller
    {
        IBLL.IArticleService articleService = OperateHelper.Current.serviceSession.ArticleService;
        IBLL.IVisitorService visitorService = OperateHelper.Current.serviceSession.VisitorService;
        IBLL.ICommentService commentService = OperateHelper.Current.serviceSession.CommentService;

        #region 1.0 导向文章详情页,异步加载文章评论; ActionResult Index(int id)
        /// <summary>
        /// 1.0 根据id获取博文详情> 
        /// 同时根据id获取上一篇下一篇> 
        /// 获取游客cookie信息> 
        /// 并判断是否是从邮件处链接过来（flag）> 
        /// 同时为该收件人进行自动登录,以便其回复/评论
        /// </summary>
        /// <param name="id">ArticleId</param>
        public ActionResult Index(int id)
        {
            #region 0.0 从邮件过来的url?页码&页大小&该条评论Id()..,配合定位到评论锚点标记位
            string flag = Request["Flag"];
            flag = Common.Security.Base64TextUTF8Decode(flag);
            if (flag == "1")
            {
                ViewBag.Flag = flag;
                ViewBag.Vid = Common.Security.Base64TextUTF8Decode(Request["Vid"]); //游客id（收件人点击了链接）
                ViewBag.AnchorIndex = Common.Security.Base64TextUTF8Decode(Request["AnchorIndex"]);
                ViewBag.AnchorSize = Common.Security.Base64TextUTF8Decode(Request["AnchorSize"]);
                ViewBag.AnchorCmtRootId = Common.Security.Base64TextUTF8Decode(Request["AnchorCmtRootId"]);
                ViewBag.AnchorCmtParentId = Common.Security.Base64TextUTF8Decode(Request["AnchorCmtParentId"]);
                ViewBag.AnchorCmtId = Common.Security.Base64TextUTF8Decode(Request["AnchorCmtId"]);

                //为链接过来的收件人设置cookie
                //先将原来的cookie删除,再赋新值，Index后续会读取这个cookie
                HttpCookie visitorCookie = HttpContext.Request.Cookies["visitorCookie"];
                if (visitorCookie == null)
                {
                    visitorCookie = new HttpCookie("visitorCookie");
                }
                visitorCookie.Value = ViewBag.Vid;  //存游客id值
                visitorCookie.Expires = DateTime.Now.AddDays(30); //设置过期时间，往后顺延30天
                HttpContext.Response.Cookies.Add(visitorCookie);
            }
            #endregion

            //获取文章详情------------------------------------------------------------------
            Article article = new Article();

            //1.1 根据博文id取数据，并判断是否为空
            article = articleService.GetEntity(id);

            //如果前台正在浏览这篇文章，这是后台却把这篇文章删了；此时浏览者刷新页面时就会出错
            //所以判断一下是否为null

            //1.2获取博文关键词  关键词可为空
            string[] keywords = null;
            if (!string.IsNullOrEmpty(article.Keywords))
            {
                //关键词规则约定：可以空格或,或，或、分开
                keywords = article.Keywords.Split(' ');
            }
            ViewData["keywords"] = keywords;

            //博文所属类别的所有博文：
            List<Article> allArt = articleService.GetDataListBy(a => a.CategoryId == article.CategoryId && a.Status == 1);

            //所有此类博文的id集合
            List<int> allArtIdsList = new List<int>();
            foreach (Article item in allArt)
            {
                allArtIdsList.Add(item.Id);
            }

            //1.3 文章底部：“上一篇”“下一篇”
            //如果allArtIdsList中只有一个元素(当前博文id),则不显示上一篇、下一篇
            //if allArtIdsList.count > 1
            //如果当前article.Id是第一个元素，则显示下一篇
            //如果当前article.Id是最后一个元素，则显示上一篇
            //如果当前article.Id不是第一个元素，也不是最后一个元素，则显示上一篇、下一篇
            int preIndex = 0;
            int nextIndex = 0;

            if (allArtIdsList.Count > 1)
            {
                if (id == allArtIdsList[0])
                {
                    nextIndex = allArtIdsList[1];
                }
                else if (id == allArtIdsList.Last())
                {
                    preIndex = allArtIdsList[allArtIdsList.Count - 2];
                }
                else if (id != allArtIdsList[0] && id != allArtIdsList.Last())
                {
                    int curposition = allArtIdsList.IndexOf(id);//当前文章所在List的位置
                    preIndex = allArtIdsList[curposition - 1];
                    nextIndex = allArtIdsList[curposition + 1];
                }
            }
            ViewBag.PreIndex = preIndex;
            ViewBag.NextIndex = nextIndex;


            //1.4 此博文的所有评论总数为：
            ViewBag.CmtCount = commentService.GetDataListBy(c => c.CmtArtId == id && c.Status == 1).Count;

            //1.5 从浏览器获取访客cookie信息,
            int visitorId = 0;
            string visitorName = String.Empty;
            string visitorEmail = String.Empty;
            HttpCookie cookie = HttpContext.Request.Cookies["visitorCookie"];
            if (cookie != null)
            {
                if (int.TryParse(cookie.Value, out visitorId))
                {
                    List<Visitor> visitorlist = visitorService.GetDataListBy(v => v.Id == visitorId && v.Status == 1);
                    if (visitorlist != null && visitorlist.Count != 0)
                    {
                        visitorName = visitorlist[0].VisitorName;
                        visitorEmail = visitorlist[0].VisitorEmail;
                    }
                }
            }

            //将这三个参数绑定到评论框上的input标签中
            ViewBag.VisitorId = visitorId;
            ViewBag.VisitorName = visitorName;
            ViewBag.VisitorEmail = visitorEmail;

            //1.6 百度分享的分享内容:
            ViewBag.Share = StringHelper.StringCut(article.ContentsRaw, 100); //新浪微博只能输入104个字

            return View(article);


        }
        #endregion

        #region 2.0 文章浏览量统计;  [HttpPost] ActionResult ViewStatic()
        /// <summary>
        /// 2.0 AJAX统计文章浏览量  必须由EF先查询，再修改（如果通过过滤器统计的话，会受缓存的影响）
        /// </summary>
        [HttpPost]
        public ActionResult ViewStatic()
        {
            string artId = HttpContext.Request["artId"];
            JsonModel jsonData;
            if (!String.IsNullOrEmpty(artId) && Convert.ToInt32(artId) != 0)
            {
                int id = Convert.ToInt32(artId);

                #region ObjectStateManager 中已存在具有同一键的对象。ObjectStateManager 无法跟踪具有相同键的多个对象。
                //知道为什么会报这个错了吧，，因为你先用的articleService.GetEntity(id);这是上下文中就已经有这个
                //实体对象了，这时再根据它的Id新new一个对象，再去用articleService.update()当然会出错。因为你这儿的dbContext是线程内唯一的，所以会报“已存在具有同一键的对象”。
                //ObjectStateManager 中已存在具有同一键的对象。ObjectStateManager 无法跟踪具有相同键的多个对象。

                //Article artModelProxy = articleService.GetEntity(id);

                //Article realModel = new Article()
                //{
                //    Id = id,
                //    ViewCount = artModelProxy.ViewCount + 1
                //};


                //BlogDb4ZynEntities dbContext = new Model.BlogDb4ZynEntities();
                ////但是又出现了：修改实体时 对一个或多个实体的验证失败。加上关闭实体验证就好了
                ////关闭EF实体验证
                //dbContext.Configuration.ValidateOnSaveEnabled = false;

                //DbEntityEntry<Article> entry = dbContext.Entry<Article>(realModel);
                //entry.State = System.Data.EntityState.Unchanged;
                //entry.Property("ViewCount").IsModified = true;

                //int val = dbContext.SaveChanges(); 
                #endregion

                BlogDb4ZynEntities dbContext = new Model.BlogDb4ZynEntities();
                Article artModel = dbContext.Articles.Find(id);
                artModel.ViewCount += 1;
                int val = dbContext.SaveChanges();

                jsonData = new JsonModel()
                {
                    Status = 1,
                    Message = "浏览量计数成功"
                };
            }
            else
            {
                jsonData = new JsonModel()
                {
                    Status = 0,
                    Message = "没有参数"
                };
            }

            return Json(jsonData);
        }
        #endregion

        #region 3.0 文章点赞量统计;  [HttpPost] ActionResult PraiseStatic()
        /// <summary>
        /// 3.0 Ajax文章点赞统计 必须由EF先查询，再修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PraiseStatic()
        {
            string artId = HttpContext.Request["artId"];
            JsonModel jsonData;

            if (!String.IsNullOrEmpty(artId) && Convert.ToInt32(artId) != 0)
            {
                int id = Convert.ToInt32(artId);

                BlogDb4ZynEntities dbContext = new Model.BlogDb4ZynEntities();
                Article artModelProxy = dbContext.Articles.Find(id);
                artModelProxy.Digg += 1;
                int val = dbContext.SaveChanges();

                if (val == 1)
                {
                    jsonData = new JsonModel()
                    {
                        Status = 1,
                        Message = "点赞计数成功"
                    };
                }
                else
                {
                    jsonData = new JsonModel()
                    {
                        Status = 2,
                        Message = "服务器繁忙,请稍后再赞"
                    };
                }
            }
            else
            {
                jsonData = new JsonModel()
                {
                    Status = 0,
                    Message = "没有参数"
                };
            }

            return Json(jsonData);
        }
        #endregion
    }
}
