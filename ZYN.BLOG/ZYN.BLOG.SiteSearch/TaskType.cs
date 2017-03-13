using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.SiteSearch
{
    //创建一个枚举，用来标识队列中的信息要进行什么操作，因为增、删、更都要放到队列中
    public enum TaskType
    {
        Add,
        Update,
        Delete
    }
}
