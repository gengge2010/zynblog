using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ZYN.BLOG.BLL;
using ZYN.BLOG.Common;
using ZYN.BLOG.FormatModel;
using ZYN.BLOG.IBLL;
using ZYN.BLOG.Inject;
using ZYN.BLOG.Model;
using ZYN.BLOG.ViewModel;
using ZYN.BLOG.WebHelper;

namespace ZYN.BLOG.Controllers
{
    public class HomeController : Controller
    {
        IBLL.ICategoryService categoryService = OperateHelper.Current.serviceSession.CategoryService;
        IBLL.IArticleService articleService = OperateHelper.Current.serviceSession.ArticleService;
        IBLL.ICommentService commentService = OperateHelper.Current.serviceSession.CommentService;

        #region 1.0 导向首页; ActionResult Index(int id)
        /// <summary>
        /// 1.0 导向首页
        /// </summary>
        /// <param name="id">文章类别Id,默认0</param>
        /// <returns></returns>
        public ActionResult Index(int id)
        {
            return View(id);
        }
        #endregion

        #region 2.0 获取首页博文列表; ActionResult WrapArtList(int id)
        /// <summary>
        /// 2.0 Index页-Ajax获取首页博文列表;显示5条
        /// </summary>
        /// <param name="id">博文类别(UrlParameter.optional)</param>
        /// <returns>JSON</returns>
        public ActionResult WrapArtList(int id)
        {
            int pageIndex = Request["pageIndex"] == null ? 1 : int.Parse(Request["pageIndex"]);
            int pageSize = Request["pageSize"] == null ? 8 : int.Parse(Request["pageSize"]);

            List<Article> articleList = new List<Article>();
            int totalCount = 0;

            if (id == 0) //取所有博文，对应于“首页”<Home/Index/0>
            {
                articleList = articleService.GetPagedList(pageIndex, pageSize, a => a.Status == 1, a => a.SubTime, true);

                totalCount = articleService.GetDataListBy(a => a.Status == 1).Count; //总条数
            }
            else  //取响应板块的博文，对应于“导航栏”<Home/Index/6>
            {
                articleList = articleService.GetPagedList(pageIndex, pageSize, a => a.CategoryId == id && a.Status == 1, a => a.SubTime, true);

                totalCount = articleService.GetDataListBy(a => a.CategoryId == id && a.Status == 1).Count; //总条数
            }

            //视图模型需要类别名称和博文基本信息
            List<ArticleViewModel> articleViewList = new List<ArticleViewModel>();

            foreach (Article item in articleList)
            {
                //1.0 获取博文类别名
                string categoryName = item.Category.Name;
                //2.0 获取博文关键词  关键词可为空
                string[] keywords = null;
                List<string> keylist = new List<string>();
                if (!string.IsNullOrEmpty(item.Keywords))
                {
                    //关键词规则约定：以空格分开
                    keywords = item.Keywords.Split(' ');

                    foreach (var word in keywords)
                    {
                        if (word != " ")
                            keylist.Add(word);
                    }
                }
                //取前五个关键词
                keywords = keylist.Take(5).ToArray();

                //3.0 获取评论总数：
                int commentCount = commentService.GetDataListBy(c => c.CmtArtId == item.Id && c.Status == 1).Count;

                //4.0 构造视图模型
                ArticleViewModel artViewModel = new ArticleViewModel()
                {
                    Id = item.Id,
                    Title = item.Title,
                    SubTime = item.SubTime.ToShortDateString(),
                    CategoryName = categoryName,
                    ViewCount = item.ViewCount,
                    CommentCount = commentCount,
                    Digg = item.Digg,
                    Contents = StringHelper.StringCut(item.ContentsRaw,600),  //ContentsRaw为不包含html标签的文本
                    Keywords = keywords  //可能为空，为空则View中不显示
                };

                articleViewList.Add(artViewModel);
            }
            //5.0 构造分页html-json
            string PagerNavString = PagerHelper.GeneratePagerString(pageIndex, pageSize, totalCount);

            JsonModel jsonData = new JsonModel()
            {
                CoreData = articleViewList,
                Status = 1,
                Message = "成功",
                GotoUrl = HttpContext.Request.Url.AbsolutePath,
                PageNavStr = PagerNavString
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);//Json方法默认只接受post请求
        }
        #endregion

        #region 3.0 获取侧边栏热门文章; 分部视图 ActionResult HotArticles()
        /// <summary>
        /// 3.0 获取侧边栏热门文章,取8条(按阅读量排序)
        /// </summary>
        /// <returns>List集合</returns>
        public ActionResult HotArticles()
        {
            List<Article> hotArticlesList = articleService.GetDataListBy<int>(a => a.Status == 1, a => a.ViewCount);
            List<Article> hotArticlesViewList = new List<Article>();//视图模型
            int j = 8;
            if (hotArticlesList.Count > 0)
            {
                if (hotArticlesList.Count < 8)
                {
                    j = hotArticlesList.Count;
                }
                hotArticlesViewList = hotArticlesList.OrderByDescending(a => a.ViewCount).Take<Article>(j).ToList();
            }

            ViewData.Model = hotArticlesViewList;

            return PartialView(); //将hotArticlesViewList作为对应的View的Model发送给View
        }
        #endregion

        #region 4.0 获取侧边栏文章归档; 分部视图 ActionResult DocArchiving()
        /// <summary>
        /// 4.0 文章归档
        /// </summary>
        /// <returns></returns>
        public ActionResult DocArchiving()
        {
            var allcategorylist = categoryService.GetDataListBy(c => c.Status == 1);
            var categoryViewlist = from c in allcategorylist
                                   select new CategoryViewModel
                                   {
                                       CategoryId = c.Id,
                                       CategoryName = c.Name,
                                       ArticleCount = articleService.GetDataListBy(a => a.CategoryId == c.Id && a.Status == 1).Count
                                   };

            return PartialView(categoryViewlist.ToList<CategoryViewModel>());
        }
        #endregion

        #region  5.0 获取侧边栏最新评论; 分部视图 ActionResult NewComments()
        /// <summary>
        /// 5.0 获取侧边栏最新评论,取6条.
        /// </summary>
        /// <returns>List集合</returns>
        public ActionResult NewComments()
        {
            IBLL.ICommentService commentService = OperateHelper.Current.serviceSession.CommentService;

            List<Comment> newCommentsList = commentService.GetDataListBy(c => c.Status != 0, c => c.SubTime);
            List<CommentViewModel> cmtViewModelList = new List<CommentViewModel>(); //视图模型

            int j = 6;//取6条还是6条内
            //如果最新评论说超过6条，则：
            if (newCommentsList.Count > 0)
            {
                if (newCommentsList.Count < 6)
                {
                    j = newCommentsList.Count;
                }

                //newCommentsList = newCommentsList.Take<Comment>(j) as List<Comment>;
                newCommentsList = newCommentsList.OrderByDescending(c => c.SubTime).Take<Comment>(j).ToList();
                string iconUrl; //评论所有人的头像路径
                string defaultIconPath = ConfigurationManager.AppSettings["DefaultHeadIcon"];

                for (int i = 0; i < j; i++)
                {
                    //1.找评论所属游客的Id
                    int? cmtVisitorId = newCommentsList[i].VisitorId;  //Comment实体的VisitorId可空，因为被设计为：删除用户并不级联删除响应评论。
                    //2.如果这条评论所属的游客还存在数据库中
                    if (cmtVisitorId.HasValue)
                    {
                        //2.1 先找到它  
                        int? headiconId = newCommentsList[i].Visitor.VisitorIconId;

                        //2.2 如果这个游客对应的头像没被删除
                        if (headiconId.HasValue)
                        {
                            //找出这个头像地址
                            iconUrl = newCommentsList[i].Visitor.HeadIcon.IconURL;
                        }
                        else
                            iconUrl = defaultIconPath;  //加上~才能显示吗？www.zynblog.com
                    }
                    else
                        iconUrl = defaultIconPath;  //否则就给这条评论加一个默认头像

                    //3.0 构造视图模型
                    CommentViewModel viewmodel = new CommentViewModel()
                    {
                        CmtId = newCommentsList[i].Id,
                        CmtText = newCommentsList[i].CmtText,
                        CmtArtId = newCommentsList[i].CmtArtId,
                        CmtIconUrl = iconUrl
                    };

                    cmtViewModelList.Add(viewmodel);
                }
            }

            return PartialView(cmtViewModelList);
        }
        #endregion

        #region 6.0 获取侧边栏标签云图; 分部视图 ActionResult TagCloud()
        /// <summary>
        /// 6.0 标签云(暂时写在了cshtml)，库中tag表为空
        /// </summary>
        /// <returns></returns>
        public ActionResult TagCloud()
        {
            return PartialView();
        }
        #endregion
    }
}
