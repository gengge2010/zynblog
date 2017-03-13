using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZYN.BLOG.Common;
using ZYN.BLOG.Model;
using ZYN.BLOG.SiteSearch;
using ZYN.BLOG.ViewModel;
using ZYN.BLOG.WebHelper;

namespace ZYN.BLOG.Controllers
{
    public class SearchController : Controller
    {
        IBLL.IArticleService articleService = OperateHelper.Current.serviceSession.ArticleService;

        #region 1.0 Lucene倒排索引搜索; ActionResult Index()
        /// <summary>
        /// 1.0 联想搜索(Lucene+盘古分词)  倒排索引 索引文件位于InedxFile中>
        ///     每新创建或修改删除一篇博文就调用一下InvertedIndex.cs处理索引文件
        /// </summary>
        /// <returns>直接将搜索结果列表展示到另一个页面中</returns>
        public ActionResult Index()
        {
            //1.1 获取搜索词
            string searchWords = Request["searchWords"];
            //规定搜索词不能超过20个字
            searchWords = StringHelper.StringCut(searchWords, 20).Replace("...", "");
           
            //1.2 从Lucene中获取的搜索结果
            //SearchResult中包含Id,题目，文章纯内容，url
            List<SearchResult> list = new List<SearchResult>();
            if (!String.IsNullOrEmpty(searchWords))
            {
                list = SiteSearch.Search.SearchContent(searchWords);
            }

            //1.3 将listViewModel作为Model传到搜索页中
            List<ArticleViewModel> listViewModel = new List<ArticleViewModel>();

            if (list != null && list.Count != 0)
            {
                ViewBag.Keyword = searchWords;

                //循环这个list(排除掉status=0的文章)，构建视图模型
                foreach (SearchResult res in list)
                {
                    Article article = articleService.GetEntity(res.Id);
                    if (article.Status == 1)
                    {
                        //取关键词 约定以空格分开
                        string[] keywords = null;
                        List<string> keylist = new List<string>();
                        if (!string.IsNullOrEmpty(article.Keywords))
                        {

                            keywords = article.Keywords.Split(' ');
                            foreach (var word in keywords)
                            {
                                if (word != " ")
                                    keylist.Add(word);
                            }
                        }
                        keywords = keylist.Take(5).ToArray();

                        listViewModel.Add(new ArticleViewModel()
                        {
                            Id = article.Id,
                            Title = res.Title,
                            SubTime = article.SubTime.ToString(),
                            CategoryName = article.Category.Name,
                            ViewCount = article.ViewCount,
                            Digg = article.Digg,
                            Contents = res.Content,//从search.cs过来的是带高亮样式的内容...
                            Keywords = keywords
                        });
                    }
                }

                return View(listViewModel);
            }
            else
            {
                ViewBag.Keyword = searchWords;
                return View(listViewModel);
            }
        } 
        #endregion

        #region 2.0 jquery typeahead插件异步获取搜索热词、自动补全搜索框; ActionResult GetHotSearchItems()
        /// <summary>
        /// 2.0 jquery typeahead插件异步获取搜索热词、自动补全搜索框>
        ///     每搜索一次就将此次用户搜索的详情入库>
        ///     定时任务每隔10分钟统计一次各搜索词的次数、将统计信息更新到SearchRank表
        /// </summary>
        /// <returns>JSON</returns>
        public ActionResult GetHotSearchItems()
        {
            //2.1 先获取当前搜索词"query"
            string query = Request["query"];
            //2.2 从数据库中查询此字段的热词集合
            IBLL.ISearchRankService hotService = OperateHelper.Current.serviceSession.SearchRankService;

            //2.3 从数据库中检索出包含此搜索词的热词集合，并按搜索次数排序,将数据返回给typeahead.js
            List<SearchRank> hotList = hotService.GetDataListBy(s => s.KeyWord.Contains(query), s => s.SearchTimes);

            if (hotList != null)
            {
                var list1 = hotList.Select(h => new
                {
                    name = h.KeyWord,
                    times = h.SearchTimes
                }).ToList();

                ArrayList list2 = new ArrayList();

                int i = 1;
                foreach (var item in list1)
                {
                    list2.Add(string.Format("<a class=\"cat\" href=\"#\">{0}</a>{1}", i, item.name));
                    i++;
                }
                return Json(list2, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        } 
        #endregion
    }
}
