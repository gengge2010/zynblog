using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.IDAL
{
     public interface IDbContextFactory
     {
         DbContext GetDbContext();
     }
}
