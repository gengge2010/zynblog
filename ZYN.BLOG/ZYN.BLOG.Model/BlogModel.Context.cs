﻿//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ZYN.BLOG.Model
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class BlogDb4ZynEntities : DbContext
    {
        public BlogDb4ZynEntities()
            : base("name=BlogDb4ZynEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Article> Articles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<HeadIcon> HeadIcons { get; set; }
        public DbSet<LeaveMsg> LeaveMsgs { get; set; }
        public DbSet<PalLink> PalLinks { get; set; }
        public DbSet<SearchDetail> SearchDetails { get; set; }
        public DbSet<SearchRank> SearchRanks { get; set; }
        public DbSet<StaticFile> StaticFiles { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<WebSetting> WebSettings { get; set; }
    }
}
