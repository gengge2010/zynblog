using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.ViewModel
{
    /// <summary>
    /// 首页博文列表视图模型
    /// </summary>
    public class ArticleViewModel
    {
        /// <summary>
        /// 博文id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 博文标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 发布时间
        /// </summary>
        public string SubTime { get; set; }
        /// <summary>
        /// 博文类别名
        /// </summary>
        public string CategoryName { get; set; } 
       /// <summary>
       /// 阅读量
       /// </summary>
        public int ViewCount { get; set; }
        /// <summary>
        /// 评论量
        /// </summary>
        public int CommentCount { get; set; }
        /// <summary>
        /// 点赞量
        /// </summary>
        public int Digg { get; set; }
        /// <summary>
        /// 博文内容
        /// </summary>
        public string Contents { get; set; }
        /// <summary>
        /// 博文关键词
        /// </summary>
        public string[] Keywords { get; set; }  

    }
}
