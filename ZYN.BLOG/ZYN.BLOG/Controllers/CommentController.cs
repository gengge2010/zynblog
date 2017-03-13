using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ZYN.BLOG.Common;
using ZYN.BLOG.FormatModel;
using ZYN.BLOG.Model;
using ZYN.BLOG.WebHelper;

namespace ZYN.BLOG.Controllers
{
    public class CommentController : Controller
    {
        IBLL.IArticleService articleService = OperateHelper.Current.serviceSession.ArticleService;
        IBLL.ICommentService commentService = OperateHelper.Current.serviceSession.CommentService;
        IBLL.IVisitorService visitorService = OperateHelper.Current.serviceSession.VisitorService;
        IBLL.IHeadIconService headService = OperateHelper.Current.serviceSession.HeadIconService;

        #region 1.0 拼接留言内容,再将json返回到前台进行显示; JsonResult WrapComment()
        /// <summary>
        /// 1.0 HttpGet评论分页 采用递归的方法直接在后台拼接好Ajax评论内容及分页Html,再将json返回到前台
        /// </summary>
        /// <param name="id">博文id</param>
        /// <returns></returns>
        public JsonResult WrapComment(int id)
        {
            int pageIndex = Request["pageIndex"] == null ? 1 : int.Parse(Request["pageIndex"]);
            int pageSize = Request["pageSize"] == null ? 4 : int.Parse(Request["pageSize"]);

            //1.1 选出当前博文的该页内的一级评论
            List<Comment> topCmtList = commentService.GetPagedList(pageIndex, pageSize, c => c.CmtArtId == id && c.Status == 1 && c.ParentId == 0, c => c.SubTime, true);

            //1.2 创建评论框区域的大容器
            StringBuilder cmtHtmlStr = new StringBuilder("<ol class=\"commentlist\">");

            //1.3 递归获取各级评论,并追加到cmtHtmlString中
            PackCmt(topCmtList, cmtHtmlStr);

            //1.4 合上评论框区域的大容器,评论区域HTML拼接完毕
            cmtHtmlStr.Append("</ol>");

            //1.5 评论总Html
            string cmtHtmlString = cmtHtmlStr.ToString();

            //1.6 评论分页Html
            string cmtPagerString = string.Empty;

            //1.7 该篇博文的所有评论(连带回复)
            int totalCount = commentService.GetDataListBy(c => c.CmtArtId == id && c.Status == 1 && c.ParentId == 0).Count;//分页插件用
            if (totalCount > 0)
            {
                cmtPagerString = PagerHelper.GeneratePagerString(pageIndex, pageSize, totalCount);
                cmtPagerString = "<div class='pagination'>" + "<ul>" + cmtPagerString + "</ul>" + "</div>";
            }

            //1.8 构造json数据
            JsonModel jsonData = new JsonModel()
            {
                CoreData = cmtHtmlString,
                Status = 1,
                Message = "成功",
                GotoUrl = HttpContext.Request.Url.AbsoluteUri,
                PageNavStr = cmtPagerString
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 2.0 递归-协助WrapComment获取各级子评论 [NonAction]PackCmt(List<Comment> cmtList, StringBuilder builder)
        /// <summary>
        /// 2.0 递归获取各级子评论
        /// </summary>
        /// <param name="cmtList">评论集合</param>
        /// <param name="builder">html大字符串</param>
        /// <returns>StringBuilder 大字符串</returns>
        [NonAction]
        public void PackCmt(List<Comment> cmtList, StringBuilder builder)
        {
            foreach (Comment cmt in cmtList)
            {
                builder.Append("<li id=\"comment-" + (cmt.Id).ToString() + "\" class=\"comment even thread-even depth-1 parent\">");
                builder.Append("<article id=\"div-comment-" + (cmt.Id).ToString() + "\" class=\"comment-body\">");
                builder.Append("<footer class=\"comment-meta\">");
                builder.Append("<div class=\"comment-author vcard\">");
                //评论者头像****************如果评论者的所属人Id字段为null，这里就要判断一下，给个默认值，否则就会：未将对象引用设置到对象实例；
                //判断完是否有这个用户，还要接着判断这个用户的头像是否为空
                string defaultIconPath = ConfigurationManager.AppSettings["DefaultHeadIcon"];
                string headiconUrl = string.Empty;

                headiconUrl = cmt.Visitor == null ? defaultIconPath : (cmt.Visitor.HeadIcon == null ? defaultIconPath : cmt.Visitor.HeadIcon.IconURL);
                builder.Append("<img src=\"" + headiconUrl + "\" class=\"avatar avatar-70 photo\" height=\"60\" width=\"60\">");
                //评论者名字****************
                builder.Append("<b class=\"fn\">");
                //空值判断
                builder.Append("<a href=\"\" rel=\"external nofollow\" class=\"floor\">" + ((cmt.Visitor == null) ? ("游客_" + (cmt.Id).ToString()) : (cmt.Visitor.VisitorName)) + "</a>");
                builder.Append("</b>");
                builder.Append("<span class=\"says\">say :</span>");
                builder.Append("</div>");
                //评论日期****************
                builder.Append("<div class=\"comment-metadata\">");
                builder.Append("<a href=\"\">");
                builder.Append("<time datetime=\"" + cmt.SubTime + "\">");
                builder.Append(cmt.SubTime);
                builder.Append("</time>");
                builder.Append("</a>");
                builder.Append("</div>");
                builder.Append("</footer>");  //评论的meta信息结束：评论Id、评论者头像、姓名、评论日期
                //评论内容****************
                builder.Append("<div class=\"comment-content\">");
                builder.Append("<p>" + cmt.CmtText + "</p>");
                builder.Append("</div>");
                //评论回复按钮****************
                builder.Append("<div class=\"reply\">");
                builder.Append("<a class=\"comment-reply-link\" href=\"" + "ajax地址" + "\" onclick='return addComment.moveForm(\"div-comment-" + cmt.Id + "\"," + cmt.Id + ", \"respond\", \"" + cmt.Article.Id + "\")'>回复</a>");
                builder.Append("</div>");
                builder.Append("</article>");
                //上面一个评论框框结束；如果这条评论下没子评论了，就用</li>将其闭合；如果还有，那就遍历这个cmt的子评论，子评论还有子评论的话，继续遍历
                //一个li代表一个子评论：格式：ol→(li→article→/li)→(li→article→/li)→(li→article→/li)
                //**********里面子评论的格式: ol→li→article→ol→li→article→/li→/ol→/li→继续li→/ol结束

                //找出每条一级评论的子评论;
                List<Comment> secCmtList = commentService.GetDataListBy<DateTime>(c => c.ParentId == cmt.Id && c.Status == 1, c => c.SubTime);

                //判断子评论集合是否为空  ************运行到第三层 出错了
                if (secCmtList.Count != 0)
                {
                    //每个子评论又是一个ol，但是class为children
                    builder.Append("<ol class=\"children\">");
                    //StringBuilder sb = PackCmt(secCmtList, builder);//内执行
                    //sb.Append("</ol>");  //忽略了builder是引用类型，不用取得return值，再Append，从WrapCmt开始，引用的一直是同一个变量
                    //很多请情况下，递归函数都是void
                    //builder.Append(sb);

                    PackCmt(secCmtList, builder);
                    builder.Append("</ol>");
                }

                builder.Append(" </li>");
            }
        }
        #endregion

        #region 3.0 异步新增评论; [HttpPost] [ValidateInput(false)] ActionResult PostCmt(Comment cmt)(模型绑定)
        /// <summary>
        /// 3.0  HttpPost接收新评论、并保存到数据库中>
        ///      将新游客入库>
        ///      将cookie保存到客户端,将vid存入input隐藏域>
        /// </summary>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult PostCmt(Comment cmt)
        {
            //先判断该文章是否被删除或是否是被禁用的( artService.GetEntity()== null)。
            var list = articleService.GetDataListBy(a => a.Id == cmt.CmtArtId && a.Status == 1);
            if (list != null && list.Count != 0)
            {
                #region 0.0 用于发邮件时记录锚点
                string pageSize = Request["PageSize"];
                #endregion

                //当每次进入Archives/Index时，就根据cookie或者session将用户信息绑定到评论框上的input标签中
                //然后，提交的时候，这儿就获取标签里面的游客信息
                string visitorId = Request["VId"]; //要么为0要么为其他数值
                string visitorName = Request["VName"];  //此处trim会出错:因为前台将这俩框设置为disabled了,值不会传过来
                string visitorEmail = Request["VEmail"];

                JsonModel jsonData;

                #region 1.0 如果能取到隐藏域中的visitorId,证明游客存在
                if (!String.IsNullOrEmpty(visitorId))
                {
                    //1.0 执行 model验证 并 插入库中
                    //CmtArtId ParentId和CmtText已进行模型绑定
                    cmt.VisitorId = int.Parse(visitorId);
                    cmt.CmtText = StringHelper.ClearHtml(cmt.CmtText);
                    cmt.Status = 1;
                    cmt.SubTime = DateTime.Now;

                    int val = commentService.Add(cmt);//入库
                    if (val == 1)
                    {
                        int vid = int.Parse(visitorId);
                        //构造json数据
                        jsonData = new JsonModel()
                        {
                            //注意：linq中不能用C#的方法，它不识别,所以先把int.Parse(visitorId);提出来
                            CoreData = GenerateOneComment(visitorService.GetDataListBy(v => v.Id == vid)[0], cmt),
                            Status = 1,
                            Message = visitorId,
                        };

                        SendBlogCmtEmail(cmt, pageSize);
                    }
                    else
                    {
                        jsonData = new JsonModel()
                        {
                            CoreData = "",
                            Status = 0,
                            Message = "评论失败,请重试",
                            GotoUrl = Request.UrlReferrer.AbsoluteUri   //获取上一级url 即Archives/Index/博文id
                        };
                    }

                    SetVisitorCookie(visitorId);

                    return Json(jsonData);
                }
                #endregion
                #region 2.0 如果取不到隐藏域中的访客信息,证明是新游客
                else
                {
                    //如果隐藏域中没有visitorId，证明是新游客来访。visitorName、visitorEmail已在前台验证
                    //将新游客和游客的评论入库
                    //检验visitorName是否重复
                    visitorName = visitorName.Trim();
                    visitorEmail = visitorEmail.Trim();
                    List<Visitor> checkVisitor = visitorService.GetDataListBy(v => v.VisitorName == visitorName);
                    //如果这个昵称已被注册了，就在check邮箱是否一样，if不一样，就认为是新用户;如果一样，就认为是同一个访客
                    if (checkVisitor != null && checkVisitor.Count != 0)
                    {
                        //如果昵称一样、邮箱也一样，证明是老访客重新登录了(可能是老用户换了浏览器，或cookie过期了，需要重新"登录")
                        Visitor vi = checkVisitor.Find(c => c.VisitorEmail == visitorEmail);
                        if (vi != null)
                        {
                            cmt.VisitorId = vi.Id;
                            cmt.CmtText = StringHelper.ClearHtml(cmt.CmtText);
                            cmt.Status = 1;
                            cmt.SubTime = DateTime.Now;
                            int val = commentService.Add(cmt);//入库
                            if (val == 1)
                            {
                                //构造json数据
                                jsonData = new JsonModel()
                                {
                                    CoreData = GenerateOneComment(visitorService.GetDataListBy(v => v.Id == vi.Id)[0], cmt),
                                    Status = 1,
                                    Message = vi.Id.ToString(),
                                };

                                //发送邮件
                                SendBlogCmtEmail(cmt, pageSize);
                            }
                            else
                            {
                                jsonData = new JsonModel()
                                {
                                    CoreData = "",
                                    Status = 0,
                                    Message = "评论失败,请重试",
                                    GotoUrl = Request.UrlReferrer.AbsoluteUri   //获取上一级url 即Archives/Index/博文id
                                };
                            }
                            //向客户端写cookie
                            SetVisitorCookie(vi.Id.ToString());
                        }
                        else
                        {
                            //如果昵称一样，邮箱不一样，则认为是新注册用户，这是提示它昵称已被抢注，请换个昵称
                            jsonData = new JsonModel()
                            {
                                CoreData = cmt.CmtText,
                                Status = 2,
                                Message = "该昵称已被占用,再起个吧~"
                            };
                        }
                    }
                    else
                    {
                        //新访客
                        //用户头像，从头像表中随机抽取一个
                        int iconId = 0;
                        List<HeadIcon> headList = headService.GetDataListBy(h => h.Status == 1);
                        Random rom = new Random();
                        if (headList.Count != 0)
                        {
                            iconId = headList[rom.Next(headList.Count)].Id;
                        }

                        Visitor newVisitor = new Visitor()
                        {
                            VisitorName = visitorName,
                            VisitorEmail = visitorEmail,
                            VisitorQQ = "",
                            VisitorIconId = iconId,  //随机选一个用户头像
                            Status = 1,
                            SubTime = DateTime.Now
                        };

                        cmt.CmtText = cmt.CmtText = StringHelper.ClearHtml(cmt.CmtText); ;
                        cmt.Status = 1;
                        cmt.SubTime = DateTime.Now;

                        newVisitor.Comments.Add(cmt);

                        //根据导航属性同时向数据库插入两条数据
                        int val = visitorService.Add(newVisitor);

                        if (val == 2)
                        {
                            jsonData = new JsonModel()
                            {
                                CoreData = GenerateOneComment(newVisitor, cmt),
                                Status = 1,
                                Message = (newVisitor.Id).ToString(),
                            };

                            //向客户端写cookie
                            SetVisitorCookie((newVisitor.Id).ToString());
                            //向博主或其他游客发邮件
                            SendBlogCmtEmail(cmt, pageSize);
                        }
                        else
                        {
                            jsonData = new JsonModel()
                            {
                                Status = 0,
                                Message = "评论失败，请刷新后重试",
                                GotoUrl = Request.UrlReferrer.AbsoluteUri   //获取上一级url 即Archives/Index/博文id
                            };
                        }
                    }

                    return Json(jsonData);
                }

                #endregion
            }
            else
            {
                JsonModel jsonData = new JsonModel()
                {
                    Status = 3,
                    Message = "该文章已被删除，暂时不能进行评论、点赞等操作",
                    GotoUrl = "http://127.0.0.1:8081/error404.html"
                };
                return Json(jsonData);
            }
        }
        #endregion

        #region 4.0 敏感词过滤; [NonAction] string TextFilter(string str)
        /// <summary>
        /// 4.0 文本敏感词过滤算法 ; 把敏感词用*代替
        /// </summary>
        /// <param name="str">评论文本</param>
        /// <returns>过滤后的评论</returns>
        [NonAction]
        public string TextFilter(string str)
        {
            //************
            return str;
        }
        #endregion

        #region 5.0 生成新评论html; [NonAction] string GenerateOneComment(Visitor visitor, Comment comment)
        /// <summary>
        /// 5.0 根据游客model和评论model生成一条新评论html
        /// </summary>
        /// <param name="visitor">游客model</param>
        /// <param name="comment">评论model</param>
        /// <returns>html</returns>
        [NonAction]
        public string GenerateOneComment(Visitor visitor, Comment comment)
        {
            //先根据visitor.HeadiconId查出visitor的头像
            HeadIcon icon = headService.GetEntity(visitor.VisitorIconId.Value);

            StringBuilder sbuilder = new StringBuilder();

            #region 临时评论模板
            // @*临时评论区域*@
            sbuilder.Append("<li id=\"li-comment-" + comment.Id + "\" class=\"comment byuser " + visitor.VisitorName + "  even thread-even depth-1\">");
            sbuilder.Append("<article class=\"comment-body\" id=\"div-comment-" + comment.Id + "\">");
            sbuilder.Append("<footer class=\"comment-meta\">");
            sbuilder.Append("<div class=\"comment-author vcard\">");
            //此处最好再在基础类里加一个获取默认值的方法：默认头像，默认博文封面图片，
            string defaultIconPath = ConfigurationManager.AppSettings["DefaultHeadIcon"];

            sbuilder.Append("<img width=\"60\" height=\"60\" class=\"avatar avatar-70 photo\" src=\"" + ((icon != null) ? icon.IconURL : defaultIconPath) + "\" alt=\"" + visitor.VisitorName + "\">	");
            sbuilder.Append("<b class=\"fn\">" + visitor.VisitorName + "</b> <span class=\"says\">say :</span>");
            sbuilder.Append("</div>");
            sbuilder.Append("<div class=\"comment-metadata\">");
            sbuilder.Append("<a href=\"\">");
            sbuilder.Append(comment.SubTime);
            sbuilder.Append("</a>");
            sbuilder.Append("</div>");
            sbuilder.Append("</footer>");
            sbuilder.Append("<div class=\"comment-content\">");
            sbuilder.Append("<p>" + comment.CmtText + "</p>");
            sbuilder.Append("</div>");
            sbuilder.Append("</article>");
            sbuilder.Append("</li>");
            #endregion

            return sbuilder.ToString();
        }
        #endregion

        #region 6.0 设置cookie ["visitorCookie"]; [NonAction] void SetVisitorCookie(string cookieVal)
        /// <summary>
        /// 6.0 为游客设置cookie;cookie的key固定为"visitorCookie"
        /// </summary>
        /// <param name="cookieVal">cookie值</param>
        [NonAction]
        public void SetVisitorCookie(string cookieVal)
        {
            //为新游客设置cookie
            HttpCookie visitorCookie = new HttpCookie("visitorCookie");
            visitorCookie.Value = cookieVal;  //存游客id值
            visitorCookie.Expires = DateTime.Now.AddDays(30); //设置过期时间，往后顺延30天
            HttpContext.Response.Cookies.Add(visitorCookie);
        }
        #endregion

        #region 7.0 发送系统邮件; [NonAction] void SendBlogCmtEmail(Comment cmt, string pageSize)
        /// <summary>
        /// 7.0 为了实现点击邮件链接后直接定位到锚点>
        ///     需要根据本条评论的Id和pageSize估测本条评论位于第几页>
        ///     同时还要找到收件人Visitor的Id，一旦该收件人点击链接，就将其Id写入cookie,以便其进行评论回复操作>
        ///     收件人要么是commentService.GetEntity(cmt.ParentId);要么是博主我自己（博主的visitorId要排在第一位1）
        /// </summary>
        /// <param name="cmt">评论</param>
        /// <param name="pageSize">页大小</param>
        [NonAction]
        public void SendBlogCmtEmail(Comment cmt, string pageSize)
        {
            //7.1 获取系统邮件Key Secret
            IBLL.IWebSettingService wService = OperateHelper.Current.serviceSession.WebSettingService;
            WebSetting keySeting = wService.GetDataListBy(w => w.ConfigKey == "SystemEmailKey").FirstOrDefault();
            WebSetting secretSeting = wService.GetDataListBy(w => w.ConfigKey == "SystemEmailSecret").FirstOrDefault();

            string systemEmailName = keySeting.ConfigValue; //系统邮箱
            string systemEmailSecret = secretSeting.ConfigValue; //系统邮箱密码

            //7.2 获取评论隶属于那一篇博文
            string title = articleService.GetEntity(cmt.CmtArtId).Title;

            //7.3 取到这条评论的最根上的评论Id
            int rootcmtId = 0;
            int parentId = 0;
            Comment parentCmt = commentService.GetDataListBy(c => c.Id == cmt.ParentId).FirstOrDefault();
            Comment tempCmt = parentCmt;

            if (parentCmt != null)
            {
                //7.4 父Id
                parentId = parentCmt.Id;
                while (tempCmt != null)
                {
                    tempCmt = commentService.GetDataListBy(c => c.Id == tempCmt.ParentId).FirstOrDefault();
                    if (tempCmt != null)
                        parentCmt = tempCmt;
                }
                rootcmtId = parentCmt.Id;
            }
            else
            {
                rootcmtId = cmt.Id;
            }

            //7.5 判断这个rootcmtId位于第几页
            int pageindex = 0;
            int pagesize = Convert.ToInt32(pageSize);
            //对所有的一级评论按照时间排序即可
            List<Comment> cmtList = commentService.GetDataListBy(c => c.CmtArtId == cmt.CmtArtId && c.Status == 1 && c.ParentId == 0, c => c.SubTime);
            //判断id为rootcmtId在第几页
            int position = cmtList.FindIndex(c => c.Id == rootcmtId); //找出这个rootId在所有一级评论中的位置,
            pageindex = Math.Max(((position + 1) + pagesize - 1) / pagesize, 1); //得到的即是该root评论在第几页的

            //7.6 url参数;最终这些参数将发送到前台js进行锚点定位、高亮处理 (对url参数进行加密)
            string url = "http://127.0.0.1:8081/Archives/Index/" + cmt.CmtArtId
                         + "?Flag=" + Common.Security.Base64UTF8Encode("1")
                         + "&AnchorIndex=" + Common.Security.Base64UTF8Encode(pageindex.ToString())
                         + "&AnchorSize=" + Common.Security.Base64UTF8Encode(pageSize.ToString())
                         + "&AnchorCmtRootId=" + Common.Security.Base64UTF8Encode(rootcmtId.ToString())
                         + "&AnchorCmtParentId=" + Common.Security.Base64UTF8Encode(parentId.ToString())
                         + "&AnchorCmtId=" + Common.Security.Base64UTF8Encode(cmt.Id.ToString());

            Visitor visitor = visitorService.GetEntity(cmt.VisitorId);

            //7.7 构造邮件主题、内容、发送邮件
            if (cmt.ParentId == 0)
            {
                string subject = "[您的博客有新评论]Re:" + title;

                url += "&Vid=" + Common.Security.Base64UTF8Encode("1");

                Visitor blogger = visitorService.GetEntity(1); //Id=3的是博主=我

                string emailBody = @"#Re: " + title
                    + "<br/>"
                    + "博客新评论："
                    + "<br/>"
                    + "内容:" + cmt.CmtText
                    + "<hr/>"
                    + "评论者：<a href='#' >" + visitor.VisitorName + "</a>"
                    + "<br/>"
                    + "URL："
                    + "<a href='" + url + "' title='链接地址'>" + url + "</a>"
                    + "<br/>"
                    + "(系统通知,请勿回复)";

                SendMail.SendEMail("smtp.126.com", systemEmailName, systemEmailSecret, blogger.VisitorEmail, subject, emailBody);
            }
            else
            {
                Comment ParentCmt = commentService.GetEntity(cmt.ParentId);

                string toEmail = ParentCmt.Visitor.VisitorEmail;
                string subject = "[您的博客评论有新回复]Re:" + title;

                url += "&Vid=" + ParentCmt.Visitor.Id;

                string emailBody = @"#Re: " + title
                        + "<br/>"
                        + "<a href='#'>@ </a>" + ParentCmt.Visitor.VisitorName
                        + "<br/>"
                        + "内容：" + cmt.CmtText
                        + "<hr/>"
                        + "回复者：<a href='#' >" + visitor.VisitorName + "</a>"
                        + "<br/>"
                        + "URL："
                        + "<a href='" + url + "' title='链接地址'>" + url + "</a>"
                        + "<br/>"
                        + "(系统通知,请勿回复)";

                SendMail.SendEMail("smtp.126.com", systemEmailName, systemEmailSecret, toEmail, subject, emailBody);
            }
        }
        #endregion
    }
}
