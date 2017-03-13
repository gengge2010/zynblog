using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.ViewModel
{
    /// <summary>
    /// 首页侧边栏最新评论视图模型
    /// </summary>
    public class CommentViewModel
    {
        public int CmtId { get; set; } //评论Id
        public string CmtText { get; set; } //评论内容
        public int CmtArtId { get; set; }  //评论对应博文的Id
        public string CmtIconUrl { get; set; } //评论所对应的Visitor的头像地址
    }
}
