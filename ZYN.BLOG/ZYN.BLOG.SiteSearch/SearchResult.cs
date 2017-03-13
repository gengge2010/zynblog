using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.SiteSearch
{
    /// <summary>
    /// 搜索结果
    /// </summary>
    public class SearchResult
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
    }
}
