using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.SiteSearch
{
    //创建一个索引任务类
    public class IndexTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Keywords { get; set; }
        public string Content { get; set; }
        public TaskType Type { get; set; }
    }
}
