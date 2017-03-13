using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ZYN.BLOG.Common;
using ZYN.BLOG.FormatModel;
using ZYN.BLOG.Model;
using ZYN.BLOG.WebHelper;

namespace ZYN.BLOG.Controllers
{
    public class LeaveMsgController : Controller
    {
        IBLL.ILeaveMsgService leaveService = OperateHelper.Current.serviceSession.LeaveMsgService;
        IBLL.IVisitorService visitorService = OperateHelper.Current.serviceSession.VisitorService;
        IBLL.IHeadIconService headService = OperateHelper.Current.serviceSession.HeadIconService;

        #region 1.0 导向留言板页面,异步加载所有留言; ActionResult Index(int id)
        /// <summary>
        /// 1.0 id只是用来记录从邮件的留言锚点
        ///     同ArchivesController一样
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Index(int id)
        {
            #region 0.0 从邮件过来的留言锚点标记位 url?参数根id，父id,自己的id
            string flag = Request["Flag"];
            flag = Common.Security.Base64TextUTF8Decode(flag);
            if (flag == "1")
            {
                ViewBag.Flag = flag;
                ViewBag.Vid = Common.Security.Base64TextUTF8Decode(Request["Vid"]); //游客id（收件人点击了链接）
                ViewBag.AnchorLvmRootId = Common.Security.Base64TextUTF8Decode(Request["AnchorLvmRootId"]);
                ViewBag.AnchorLvmParentId = Common.Security.Base64TextUTF8Decode(Request["AnchorLvmParentId"]);
                ViewBag.AnchorLvmId = Common.Security.Base64TextUTF8Decode(Request["AnchorLvmId"]);

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

            //1.1 获取留言总数
            ViewBag.lvmCount = leaveService.GetDataListBy(lm => lm.Status == 1 && lm.ParentId==0).Count;

            //1.2 从浏览器获取访客cookie信息,
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

            //将这三个参数绑定到留言框上的input标签中
            ViewBag.VisitorId = visitorId;
            ViewBag.VisitorName = visitorName;
            ViewBag.VisitorEmail = visitorEmail;

            return View();
        }
        #endregion

        #region 2.0 拼接留言内容,再将json返回到前台进行显示; JsonResult WrapLeaveMsg()
        /// <summary>
        /// 2.0 采用递归的方法直接在后台拼接好留言内容，再将json返回到前台进行显示
        /// </summary>
        /// <returns></returns>
        public JsonResult WrapLeaveMsg()
        {
            //2.1 先选出所有一级留言
            List<LeaveMsg> topLvMList = leaveService.GetDataListBy(lm => lm.Status == 1 && lm.ParentId == 0, lm => lm.SubTime);

            //2.2 创建留言框区域的大容器
            StringBuilder lvmHtmlStr = new StringBuilder("<ol class=\"commentlist\">");

            //2.3 递归获取各级留言,并追加到cmtHtmlString中
            PackCmt(topLvMList, lvmHtmlStr);

            //2.4 合上留言框区域的大容器,留言区域HTML拼接完毕
            lvmHtmlStr.Append("</ol>");

            //2.5 留言总Html
            string lvmHtmlString = lvmHtmlStr.ToString();

            //2.6 构造json数据
            JsonModel jsonData = new JsonModel()
            {
                CoreData = lvmHtmlString,
                Status = 1,
                Message = "成功",
                GotoUrl = HttpContext.Request.Url.AbsoluteUri
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 3.0 获取留言的递归方法 [NonAction] PackCmt(List<LeaveMsg> lmList, StringBuilder builder)
        /// <summary>
        /// 3.0 递归获取各级子留言
        /// </summary>
        /// <param name="lmList">一级留言(parentId=0)的集合</param>
        /// <param name="builder">html大字符串</param>
        /// <returns>StringBuilder 大字符串</returns>
        [NonAction]
        public void PackCmt(List<LeaveMsg> lmList, StringBuilder builder)
        {
            #region 递归遍历当前页一级留言下的所有子留言
            int i = 1; //楼层
            foreach (LeaveMsg lmsg in lmList)
            {
                builder.Append("<li id=\"comment-" + (lmsg.Id).ToString() + "\" class=\"comment even thread-even depth-1 parent\">");
                builder.Append("<article id=\"div-comment-" + (lmsg.Id).ToString() + "\" class=\"comment-body\">");
                builder.Append("<footer class=\"comment-meta\">");
                builder.Append("<div class=\"comment-author vcard\">");

                //留言头像******如果留言者的所属人Id字段为null，这里就要判断一下，给个默认值，否则就会：未将对象引用设置到对象实例；
                //判断完是否有这个用户，还要接着判断这个用户的头像是否为空
                string defaultIconPath = ConfigurationManager.AppSettings["DefaultHeadIcon"];
                string headiconUrl = string.Empty;
                headiconUrl = lmsg.Visitor == null ? defaultIconPath : (lmsg.Visitor.HeadIcon == null ? defaultIconPath : lmsg.Visitor.HeadIcon.IconURL);
                builder.Append("<img src=\"" + headiconUrl + "\" class=\"avatar avatar-70 photo\" height=\"60\" width=\"60\">");
                //留言者名字****************
                builder.Append("<b class=\"fn\">");
                //空值判断
                builder.Append("<a href=\"\" rel=\"external nofollow\" class=\"floor\">" + ((lmsg.Visitor == null) ? ("游客_" + (lmsg.Id).ToString()) : (lmsg.Visitor.VisitorName)) + "</a>");
                builder.Append("</b>");

                builder.Append("&nbsp;&nbsp;&nbsp;&nbsp;");

                //留言日期****************
                builder.Append("<a href=\"\">");
                builder.Append("<time datetime=\"" + lmsg.SubTime + "\">");
                builder.Append(lmsg.SubTime);
                builder.Append("</time>");
                builder.Append("</a>");
                builder.Append("</div>");

                if (lmsg.ParentId == 0)
                {
                    builder.Append("<div class=\"comment-metadata\" style=\"color:#D2322D\">");
                    builder.Append("#" + i.ToString());
                    builder.Append("</div>");

                    i++;
                }

                builder.Append("</footer>");  //留言的meta信息结束：留言Id、留言者头像、姓名、留言日期
                //留言内容****************
                builder.Append("<div class=\"comment-content\">");
                builder.Append("<p>" + lmsg.LMessage + "</p>");
                builder.Append("</div>");

                //留言回复按钮****************有问题****************************************************
                builder.Append("<div class=\"reply\">");
                builder.Append("<a class=\"comment-reply-link\" href=\"" + "ajax地址" + "\" onclick='return addComment.moveForm(\"div-comment-" + lmsg.Id + "\"," + lmsg.Id + ", \"respond\", \"" + lmsg.Id + "\")'>回复</a>");
                builder.Append("</div>");
                builder.Append("</article>");

                //找出每条一级留言的子留言;
                List<LeaveMsg> secLvmList = leaveService.GetDataListBy<DateTime>(lm => lm.Status == 1 && lm.ParentId == lmsg.Id, lm => lm.SubTime);


                //判断子留言集合是否为空  
                if (secLvmList.Count != 0)
                {
                    //每个子留言又是一个ol，但是class为children
                    builder.Append("<ol class=\"children\">");

                    PackCmt(secLvmList, builder);
                    builder.Append("</ol>");
                }

                builder.Append(" </li>");
            }
            #endregion
        }
        #endregion

        #region 4.0 将新留言新回复入库 [HttpPost] [ValidateInput(false)] ActionResult PostLvm(LeaveMsg lvm)
        /// <summary>
        /// 4.0 Ajax-post接收新留言、并保存到数据库中>
        ///     同时将新访客入库>
        ///     将游客cookie写入浏览器
        /// </summary>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult PostLvm(LeaveMsg lvm)
        {
            //当每次进入Archives/Index时，就根据cookie或者session将用户信息绑定到留言框上的input标签中
            //然后，提交的时候，这儿就获取标签里面的游客信息
            string visitorId = Request["VId"];
            string visitorName = Request["VName"];
            string visitorEmail = Request["VEmail"];

            JsonModel jsonData;

            #region 1.0 如果能取到隐藏域中的visitorId,证明游客存在;则直接将留言入库
            if (!String.IsNullOrEmpty(visitorId))
            {
                //1.0 执行 model验证 并 插入库中
                //ParentId和LMessage已进行模型绑定
                lvm.VisitorId = int.Parse(visitorId);
                lvm.LMessage = StringHelper.ClearHtml(lvm.LMessage); //清除html
                lvm.Status = 1;
                lvm.SubTime = DateTime.Now;

                int val = leaveService.Add(lvm);//入库
                if (val == 1)
                {
                    int vid = int.Parse(visitorId);
                    //构造json数据
                    jsonData = new JsonModel()
                    {
                        CoreData = GenerateOneLeaveMsg(visitorService.GetDataListBy(v => v.Id == vid)[0], lvm),
                        Status = 1,
                        Message = visitorId,
                    };
                }
                else
                {
                    //构造json数据
                    jsonData = new JsonModel()
                    {
                        CoreData = "",
                        Status = 0,
                        Message = "留言失败,请重试",
                        GotoUrl = Request.UrlReferrer.AbsoluteUri   //获取上一级url
                    };
                }

                SetVisitorCookie(visitorId);
                SendBlogLvmEmail(lvm);
                return Json(jsonData);
            }
            #endregion
            #region 2.0 如果取不到隐藏域中的访客信息,则先生成一个访客（根据输入信息判断是否为老游客），再生成留言
            else
            {
                //检验visitorName、visitorEmail。放到前台验证是否为空，邮箱格式是否正确
                visitorName = visitorName.Trim();
                visitorEmail = visitorEmail.Trim();
                List<Visitor> checkVisitor = visitorService.GetDataListBy(v => v.VisitorName == visitorName);
                if (checkVisitor != null && checkVisitor.Count != 0)
                {
                    Visitor vi = checkVisitor.Find(c => c.VisitorEmail == visitorEmail);
                    if (vi != null)
                    {
                        lvm.VisitorId = vi.Id;
                        lvm.LMessage = StringHelper.ClearHtml(lvm.LMessage);
                        lvm.Status = 1;
                        lvm.SubTime = DateTime.Now;

                        int val = leaveService.Add(lvm);//入库
                        if (val == 1)
                        {
                            //构造json数据
                            jsonData = new JsonModel()
                            {
                                CoreData = GenerateOneLeaveMsg(visitorService.GetDataListBy(v => v.Id == vi.Id)[0], lvm),
                                Status = 1,
                                Message = vi.Id.ToString(),
                            };
                            //发邮件
                            SendBlogLvmEmail(lvm);
                        }
                        else
                        {
                            //构造json数据
                            jsonData = new JsonModel()
                            {
                                CoreData = "",
                                Status = 0,
                                Message = "留言失败,请重试",
                                GotoUrl = Request.UrlReferrer.AbsoluteUri   //获取上一级url
                            };
                        }

                        SetVisitorCookie(vi.Id.ToString());

                    }
                    else
                    {
                        //如果昵称一样，邮箱不一样，则认为是新注册用户，这是提示它昵称已被抢注，请换个昵称
                        jsonData = new JsonModel()
                        {
                            CoreData = lvm.LMessage,
                            Status = 2,
                            Message = "该昵称已被占用,再起个吧~"
                        };
                    }
                }
                else
                {
                    //Add新游客
                    if (String.IsNullOrEmpty(visitorEmail))
                    {
                        //构造json数据
                        jsonData = new JsonModel()
                        {
                            CoreData = "",
                            Status = 2,   //此处要根据错误类型定制弹出消息，以便js进行弹出框类型处理
                            //0:留言失败，请刷新后重试；1:成功；2:邮箱错误
                            Message = "请输入邮箱后再发表留言",
                        };
                    }
                    else
                    {
                        //将新游客和游客的留言入库
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

                        //lvmArtId ParentId和lvmText已进行模型绑定
                        lvm.LMessage = lvm.LMessage = StringHelper.ClearHtml(lvm.LMessage); ;
                        lvm.Status = 1;
                        lvm.SubTime = DateTime.Now;

                        //向Visitor model的导航属性实体集中追加一个model
                        newVisitor.LeaveMsgs.Add(lvm);

                        //根据导航属性同时向数据库插入两条数据
                        int val = visitorService.Add(newVisitor);
                        if (val == 2)
                        {
                            jsonData = new JsonModel()
                            {
                                CoreData = GenerateOneLeaveMsg(newVisitor, lvm),
                                Status = 1,
                                Message = (newVisitor.Id).ToString(),
                            };

                            SetVisitorCookie((newVisitor.Id).ToString());
                            SendBlogLvmEmail(lvm);
                        }
                        else
                        {
                            jsonData = new JsonModel()
                            {
                                Status = 0,
                                Message = "留言失败，请刷新后重试",
                                GotoUrl = Request.UrlReferrer.AbsoluteUri   //获取上一级url
                            };
                        }
                    }
                }

                return Json(jsonData);
            }
            #endregion
        }
        #endregion

        #region 5.0 生成新回复html;  [NonAction] string GenerateOneLeaveMsg(Visitor visitor, LeaveMsg lvm)
        /// <summary>
        /// 5.0 根据游客model和回复model生成一条新回复html
        /// </summary>
        /// <param name="visitor">游客model</param>
        /// <param name="comment">回复model</param>
        /// <returns>html</returns>
        [NonAction]
        public string GenerateOneLeaveMsg(Visitor visitor, LeaveMsg lvm)
        {
            //先根据visitor.HeadiconId查出visitor的头像
            HeadIcon icon = headService.GetEntity(visitor.VisitorIconId.Value);

            StringBuilder sbuilder = new StringBuilder();

            #region 临时回复模板
            // @*临时回复区域*@
            sbuilder.Append("<li id=\"li-comment-" + lvm.Id + "\" class=\"comment byuser " + visitor.VisitorName + "  even thread-even depth-1\">");
            sbuilder.Append("<article class=\"comment-body\" id=\"div-comment-" + lvm.Id + "\">");
            sbuilder.Append("<footer class=\"comment-meta\">");
            sbuilder.Append("<div class=\"comment-author vcard\">");
            //此处最好再在基础类里加一个获取默认值的方法：默认头像，默认博文封面图片，
            string defaultIconPath = ConfigurationManager.AppSettings["DefaultHeadIcon"];
            sbuilder.Append("<img width=\"60\" height=\"60\" class=\"avatar avatar-70 photo\" src=\"" + ((icon != null) ? icon.IconURL : defaultIconPath) + "\" alt=\"" + visitor.VisitorName + "\">	");
            sbuilder.Append("<b class=\"fn\">" + visitor.VisitorName + "</b> <span class=\"says\">say :</span>");
            sbuilder.Append("</div>");
            sbuilder.Append("<div class=\"comment-metadata\">");
            sbuilder.Append("<a href=\"\">");
            sbuilder.Append(lvm.SubTime);
            sbuilder.Append("</a>");
            sbuilder.Append("</div>");
            sbuilder.Append("</footer>");
            sbuilder.Append("<div class=\"comment-content\">");
            sbuilder.Append("<p>" + lvm.LMessage + "</p>");
            sbuilder.Append("</div>");
            sbuilder.Append("</article>");
            sbuilder.Append("</li>");
            #endregion

            return sbuilder.ToString();
        }
        #endregion

        #region 6.0 设置cookie["visitorCookie"];  [NonAction] void SetVisitorCookie(string cookieVal)
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

        #region 7.0 发送邮件; [NonAction] void SendBlogLvmEmail(LeaveMsg lvm)
        /// <summary>
        ///  7.0 给博主或被回复人发送邮件
        /// </summary>
        /// <param name="cmt">新 评论实体</param>
        [NonAction]
        public void SendBlogLvmEmail(LeaveMsg lvm)
        {
            //7.1 获取系统邮件Key Secret
            IBLL.IWebSettingService wService = OperateHelper.Current.serviceSession.WebSettingService;
            WebSetting keySeting = wService.GetDataListBy(w => w.ConfigKey == "SystemEmailKey").FirstOrDefault();
            WebSetting secretSeting = wService.GetDataListBy(w => w.ConfigKey == "SystemEmailSecret").FirstOrDefault();

            string systemEmailName = keySeting.ConfigValue; //系统邮箱
            string systemEmailSecret = secretSeting.ConfigValue; //系统邮箱密码

            //7.2 先取到这条留言的最根上的留言Id
            int rootlvmId = 0;
            int parentId = 0;
            LeaveMsg parentLvm = leaveService.GetDataListBy(c => c.Id == lvm.ParentId).FirstOrDefault();
            LeaveMsg tempLvm = parentLvm;

            if (parentLvm != null)
            {
                //7.3 父Id
                parentId = parentLvm.Id;
                while (tempLvm != null)
                {
                    tempLvm = leaveService.GetDataListBy(c => c.Id == tempLvm.ParentId).FirstOrDefault();
                    if (tempLvm != null)
                        parentLvm = tempLvm;
                }
                rootlvmId = parentLvm.Id;
            }
            else
            {
                rootlvmId = lvm.Id;
            }

            //7.4 url参数;最终这些参数将发送到前台js进行锚点定位、高亮处理
            string url = "http://127.0.0.1:8081/LeaveMsg/Index/"
                        + "?Flag=" + Common.Security.Base64UTF8Encode("1")
                        + "&AnchorLvmRootId=" + Common.Security.Base64UTF8Encode(rootlvmId.ToString())
                        + "&AnchorLvmParentId=" + Common.Security.Base64UTF8Encode(parentId.ToString())
                        + "&AnchorLvmId=" + Common.Security.Base64UTF8Encode(lvm.Id.ToString());//Ajax加载完数据后定位到锚点

            //7.5 构造邮件主题、内容、发送邮件
            Visitor visitor = visitorService.GetEntity(lvm.VisitorId);
            if (lvm.ParentId == 0)
            {
                //给博主的留言
                string subject = "[您的博客有新留言]Re:留言板";

                url += "&Vid=" + Common.Security.Base64UTF8Encode("1");

                Visitor blogger = visitorService.GetEntity(1); //Id=3的是博主=我

                string emailBody = @"#Re: 留言板"
                    + "<br/>"
                    + "新留言："
                    + "<br/>"
                    + "内容:" + lvm.LMessage
                    + "<hr/>"
                    + "留言者：<a href='#' >" + visitor.VisitorName + "</a>"
                    + "<br/>"
                    + "URL："
                    + "<a href='" + url + "' title='链接地址'>" + url + "</a>"
                    + "<br/>"
                    + "(系统通知,请勿回复)";

                SendMail.SendEMail("smtp.126.com", systemEmailName, systemEmailSecret, blogger.VisitorEmail, subject, emailBody);
            }
            else
            {
                //游客给其它留言者的回复
                LeaveMsg parentlvm = leaveService.GetEntity(lvm.ParentId);

                string toEmail = parentlvm.Visitor.VisitorEmail;
                string subject = "[zynblog留言新回复]Re:";

                url += "&Vid=" + parentlvm.Visitor.Id;

                string emailBody = @"#Re: zynblog留言板"
                        + "<br/>"
                        + "<a href='#'>@ </a>" + parentlvm.Visitor.VisitorName
                        + "<br/>"
                        + "内容：" + lvm.LMessage
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
