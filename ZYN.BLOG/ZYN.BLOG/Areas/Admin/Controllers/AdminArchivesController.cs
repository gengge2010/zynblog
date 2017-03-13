using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ZYN.BLOG.Common;
using ZYN.BLOG.jqDataTableModel;
using ZYN.BLOG.Model;
using ZYN.BLOG.SiteSearch;
using ZYN.BLOG.WebHelper;

namespace ZYN.BLOG.Areas.Admin.Controllers
{
    public class AdminArchivesController : BaseController
    {
        IBLL.IArticleService articleService = WebHelper.OperateHelper.Current.serviceSession.ArticleService;
        IBLL.ICommentService commentService = WebHelper.OperateHelper.Current.serviceSession.CommentService;
        IBLL.ICategoryService categoryService = WebHelper.OperateHelper.Current.serviceSession.CategoryService;
        IBLL.IStaticFileService fileService = OperateHelper.Current.serviceSession.StaticFileService;

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 写博客
        /// </summary>
        /// <returns></returns>
        public ActionResult AddArchive()
        {
            //类别列表
            var list = categoryService.GetDataListBy(c => c.Status == 1);
            SelectList selList = new SelectList(list, "Id", "Name");
            ViewBag.SelList = selList.AsEnumerable<SelectListItem>();

            return View();
        }
        /// <summary>
        /// http-post：新增博客
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddArchive(Article model)
        {
            //验证model
            string nohtmlContents = StringHelper.ClearHtml(model.Contents);
            model.ContentsRaw = nohtmlContents;

            model.Digg = 0;
            model.ViewCount = 0;
            model.SubTime = DateTime.Now;

            //如果在编辑的过程中又新增了图片或文件怎么办？ 编辑调用的是Update方法，不是Add方法
            //或者在编辑的过程中删除了某图片，怎么办？ 怎么同时把七牛的文件也删除？<>
            //在文件的增、删中，还得像索引文件的增、删一样，建立线程安全的队列进行处理
            int val1 = articleService.Add(model);

            if (val1 == 1)
            {
                //Lucene 新增索引
                InvertedIndex.IndexManager.Add(model.Id, model.Title, model.ContentsRaw);

                var filelist = WebHelper.OperateHelper.Current.Session["ArticleFiles"] as List<StaticFile>;
                if (filelist != null && filelist.Count != 0)
                {
                    foreach (StaticFile file in filelist)
                    {
                        file.FromArticleId = model.Id;
                        fileService.Update(file, "FromArticleId");
                    }

                    filelist.Clear();     //清空Session["ArticleFiles"]
                }

                //返回json
                return Json(new
                {
                    Status = 1,
                    Message = "新增成功"
                });
            }

            else
                return Json(new
                {
                    Status = 0,
                    Message = "新增失败"
                });
        }

        /// <summary>
        /// 2.0 jquery.datatable()插件 Ajax Get文章列表
        /// </summary>
        /// <returns>datatable-JSON</returns>
        public JsonResult GetArchivesJson(jqDataTableParameter tableParam)
        {
            #region 1.0 场景一:服务端一次性取出所有数据,完全由客户端来处理这些数据.此时"bServerSide": false,
            ////1. 获取所有文章
            //List<Article> DataSource = articleService.GetDataListBy(a => true, a => a.Id);
            ////2. 构造aaData
            //var data = DataSource.Select(a => new object[]{
            //  a.Id,
            //  a.Title+ "  ("+a.SubTime.ToString()+")",
            //  (categoryService.GetDataListBy(c=>c.Id==a.CategoryId)[0]).Name,
            //  a.ViewCount,
            //  commentService.GetDataListBy(c=>c.CmtArtId==a.Id).Count,
            //  a.Digg,
            //  a.Status==1?"正常":"删除"

            //});
            ////3. 返回json,aaData是一个数组，数组里面还是字符串数组
            //return Json(new
            //{
            //    sEcho = 1,
            //    iTotalRecords = DataSource.Count,
            //    iTotalDisplayRecords = data.Count(),
            //    aaData = data
            //}, JsonRequestBehavior.AllowGet); 
            #endregion

            #region 2.0 场景二:服务端处理分页后数据,客户端呈现,此时为true
            //客户端需要"bServerSide": true, 用mDataProp绑定字段,obj.aData.Id获取字段(.属性)

            //0.0 全部数据
            List<Article> DataSource = articleService.GetDataListBy(a => true);
            //DataSource = DataSource.OrderByDescending(a => a.SubTime).ToList();

            //1.0 首先获取datatable提交过来的参数
            string echo = tableParam.sEcho;  //用于客户端自己的校验
            int dataStart = tableParam.iDisplayStart;//要请求的该页第一条数据的序号
            int pageSize = tableParam.iDisplayLength == -1 ? DataSource.Count : tableParam.iDisplayLength;//每页容量（=-1表示取全部数据）
            string search = tableParam.sSearch;

            //2.0 根据参数(起始序号、每页容量、参训参数)查询数据
            if (!String.IsNullOrEmpty(search))
            {
                var data = DataSource.Where(a => a.Title.Contains(search) ||
                                 a.Keywords.Contains(search) ||
                                 a.Contents.Contains(search))
                     .Skip<Article>(dataStart)
                     .Take(pageSize)
                     .Select(a => new
                     {
                         Id = a.Id,
                         Title = a.Title + "  (" + a.SubTime.ToString() + ")",
                         CategoryName = a.Category.Name,
                         ViewCount = a.ViewCount,
                         CommentCount = commentService.GetDataListBy(c => c.CmtArtId == a.Id).Count,
                         Digg = a.Digg,
                         Status = a.Status
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
                var data = DataSource.Skip<Article>(dataStart)
                                     .Take(pageSize)
                                     .Select(a => new
                                     {
                                         Id = a.Id,
                                         Title = a.Title + "  (" + a.SubTime.ToString() + ")",
                                         CategoryName = a.Category.Name,
                                         ViewCount = a.ViewCount,
                                         CommentCount = commentService.GetDataListBy(c => c.CmtArtId == a.Id).Count,
                                         Digg = a.Digg,
                                         Status = a.Status
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
            #endregion
        }

        /// <summary>
        /// 3.1 编辑文章.为文章编辑页的Http-Get请求返回文章详情
        /// </summary>
        /// <returns>文章视图model</returns>
        public ActionResult EditArchive(int id)
        {
            Article article = articleService.GetEntity(id);
            //解码 "代码格式中的空格什么的？"
            //article.Contents = HttpUtility.HtmlDecode(article.Contents);

            //json把换行解析成\r\n;而UEditor将\r和\n都解析成了换行。
            //这样原来只有一个换行的就变成了两个换行。
            //所以在初始化百度编辑器之前，用js字符串替换一下就可以了

            JavaScriptSerializer jss = new JavaScriptSerializer();
            string json = jss.Serialize(article.Contents.Replace("\"", " '"));  //先把Contents属性里面的C#对"的转义换为';以免对img src= href=造成影响。完美 perfect!

            //去除json中的首尾部""引号，
            json = json.Substring(1, json.Length - 2);

            // json编码格式的<br>: \u003cbr/\u003e
            ViewData["json"] = json;

            List<Category> listCate = categoryService.GetDataListBy(c => c.Status == 1).ToList();
            SelectList selList = new SelectList(listCate, "Id", "Name"); //值字段:Id; 字面量:Name

            ViewBag.SelList = selList.AsEnumerable<SelectListItem>();

            return View(article);
        }

        /// <summary>
        /// 3.2 接收编辑后的文章.为文章编辑页的Http-Post请求更新入库。操作(编辑、编辑中不可修改评论数)
        /// </summary>
        /// <returns>Json</returns>
        [HttpPost]
        //对textarea中的html标签不进行验证，咱不做xss保护
        [ValidateInput(false)]
        public ActionResult EditArchive(Article model)
        {
            string nohtmlContents = StringHelper.ClearHtml(model.Contents);
            model.ContentsRaw = nohtmlContents;

            model.AltTime = DateTime.Now;
            //忘记了将百度编辑器<textarea name="">的name换成属性名:"Contents"

            #region 修改  错误  还是不懂怎么只修改相应的属性
            /*
             * EF 的修改操作共有三种方式：
             *   1. 先从上下文中将此实体查来，直接修改对应属性就行了。缺点是要连两次库
             *     BlogDb4ZynEntities dbContext = new BlogDb4ZynEntities();
             *     Article model1 = dbContext.Articles.Find(1);
             *     model1.Title = "新标题";
             *     dbContext.SaveChanges();
             *   2. 经常用到模型绑定，此时是直接new的一个新对象（属性要写全），并不是从上下文中查出的
             *     此时可以从EF 内部原理入手进行修改：先将新模型附加到上下文中，然后设置它的状态为Modified；设为.Modified也就意味着把Id为"**"的实体其它属性全部修改了，没有针对性
             *      Article model2 = new Article();
                    model2.Id = 2;
                    model2.Title = "第二个新标题";
                    DbEntityEntry entry2 = dbContext.Entry<Article>(model2);
                    entry2.State = System.Data.EntityState.Modified;
                    dbContext.SaveChanges();
             */

            #endregion

            //BlogDb4ZynEntities dbContext = new BlogDb4ZynEntities();
            //DbEntityEntry entry = dbContext.Entry<Article>(model);
            //entry.State = System.Data.EntityState.Modified;
            //int val = dbContext.SaveChanges();
            //实验表明：当使用针对性的修改属性时，把对应的字段加上就行了。：先把实体设为Unchanged，再将对应属性状态设为Modified

            int val = articleService.Update(model, "CategoryId", "Author", "Title", "Abstract", "Keywords", "Contents", "ContentsRaw", "Digg", "ViewCount", "Status", "SubTime", "AltTime");
            if (val == 1)
            {
                #region 实验：更新全部索引
                //var list = articleService.GetDataListBy(a => true);
                //foreach (var item in list)
                //{
                //    InvertedIndex.IndexManager.Update(item.Id, item.Title, StringHelper.ClearHtml(item.Contents));
                //} 
                #endregion

                //Lucene 更新索引
                InvertedIndex.IndexManager.Update(model.Id, model.Title, model.ContentsRaw);

                //如果在编辑的过程中又新增了图片或文件怎么办？ 编辑调用的是Update方法，不是Add方法
                //或者在编辑的过程中删除了某图片，怎么办？ 怎么同时把七牛的文件也删除？
                //在文件的增、删中，还得像索引文件的增、删一样，建立线程安全的队列进行处理
                var filelist = WebHelper.OperateHelper.Current.Session["ArticleFiles"] as List<StaticFile>;
                if (filelist != null && filelist.Count != 0)
                {
                    foreach (var file in filelist)
                    {
                        file.FromArticleId = model.Id;
                        fileService.Update(file, "FromArticleId");
                    }
                    filelist.Clear();     //清空Session["ArticleFiles"]
                }

                //返回json
                return Json(new
                {
                    Status = 1,
                    Message = "保存成功"
                });
            }

            else
                return Json(new
                {
                    Status = 0,
                    Message = "咦?出错了"
                });
        }

        /// <summary>
        /// 4.0 根据id删除某篇文章(软删除)
        /// </summary>
        [HttpPost]
        public ActionResult DeleteArchive(int id)
        {
            Article model = new Article()
            {
                Id = id,
                Status = 0,
            };

            int val = articleService.Update(model, "Status");

            if (val == 1)
                return Json(1);
            else
                return Json(0);
        }

        /// <summary>
        /// 5.0 根据id将文章设为发布状态
        /// </summary>
        [HttpPost]
        public ActionResult PublicArchive(int id)
        {
            Article model = new Article()
            {
                Id = id,
                Status = 1
            };
            int val = articleService.Update(model, "Status");

            if (val == 1)
                return Json(1);
            else
                return Json(0);
        }

        /// <summary>
        /// 6.0 真删除文章，同时数据库会级联删除外键属性。同时删除索引文件，否则搜索时会报错
        /// </summary>
        /// <param name="id">文章ID</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RealDelArchive(int id)
        {
            //1.0 删除文章 （外键评论也会被删除）
            int val1 = articleService.Delete(id);//直接用自己写好的方法进行删除失败的原因是dbContext 乱了。。。垃圾框架！
            if (val1 == 1)
            {
                var fileList = fileService.GetDataListBy(f => f.FromArticleId == id);

                List<string> FilePathList = new List<string>();//暂存将被删的文件路径；用于删完文件表后再删七牛数据

                //2.0 删除文件表

                if (fileList.Count != 0 && fileList != null)
                {
                    foreach (var file in fileList)
                    {
                        FilePathList.Add(file.FileOnLineURL);
                    }

                    int totalFile = 0;
                    foreach (var file in fileList)
                    {
                        //2.0 删除文件表
                        int val2 = fileService.Delete(file.Id);
                        if (val2 == 1)
                        {
                            totalFile += 1;
                        }
                    }

                    //3.0 删七牛文件
                    if (totalFile == fileList.Count)
                    {
                        foreach (string path in FilePathList)
                        {
                            string bucket = ConfigurationManager.AppSettings["QiNiuURL"];
                            string key = path;
                            bool flag = QiniuHelper.DeleteFile(bucket, key);
                        }
                    }
                }

                //2.0 删除索引以及返回jsoon
                InvertedIndex.IndexManager.Delete(id);
                return Json(val1);
            }
            else
            {
                return Json(0);
            }
        }
    }
}
