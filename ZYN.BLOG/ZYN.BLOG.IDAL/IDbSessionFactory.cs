using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.IDAL
{
    //数据仓储工厂
    public interface  IDbSessionFactory
    {
        IDAL.IDbSession GetDbSession();
    }
}
