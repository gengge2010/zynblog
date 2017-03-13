using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using ZYN.BLOG.IDAL;

namespace ZYN.BLOG.DAL
{
    public class DbSessionFactory : IDbSessionFactory
    {
        //依赖接口编程，尽量返回给外界抽象的东西
        public IDbSession GetDbSession()
        {
            IDAL.IDbSession dbSession = CallContext.GetData("DbSession") as DbSession;

            int threadId1 = Thread.CurrentThread.ManagedThreadId;

            if (dbSession == null)
            {
                dbSession = new DAL.DbSession();
                CallContext.SetData("DbSession", dbSession);
            }

            return dbSession;
        }
    }
}
