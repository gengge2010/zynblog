using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.ViewModel
{
    /// <summary>
    /// 文章归档
    /// </summary>
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ArticleCount { get; set; } 
    }
}
