
 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYN.BLOG.Model;

namespace ZYN.BLOG.DAL
{
    /// <summary>
    /// 各子DAL需要实现自己的I<>DAL，同时继承BaseDAL以便拥有各子DAL共有的CURD
    /// </summary>
	public partial class ArticleDAL : BaseDAL<ZYN.BLOG.Model.Article>, IDAL.IArticleDAL
    {
	    public override void SetDbContext()
		{
			 dbContext = new BlogDb4ZynEntities();
			 dbContext.Configuration.ValidateOnSaveEnabled = false;
		}
    }

	public partial class CategoryDAL : BaseDAL<ZYN.BLOG.Model.Category>, IDAL.ICategoryDAL
    {
	    public override void SetDbContext()
		{
			 dbContext = new BlogDb4ZynEntities();
			 dbContext.Configuration.ValidateOnSaveEnabled = false;
		}
    }

	public partial class CommentDAL : BaseDAL<ZYN.BLOG.Model.Comment>, IDAL.ICommentDAL
    {
	    public override void SetDbContext()
		{
			 dbContext = new BlogDb4ZynEntities();
			 dbContext.Configuration.ValidateOnSaveEnabled = false;
		}
    }

	public partial class HeadIconDAL : BaseDAL<ZYN.BLOG.Model.HeadIcon>, IDAL.IHeadIconDAL
    {
	    public override void SetDbContext()
		{
			 dbContext = new BlogDb4ZynEntities();
			 dbContext.Configuration.ValidateOnSaveEnabled = false;
		}
    }

	public partial class LeaveMsgDAL : BaseDAL<ZYN.BLOG.Model.LeaveMsg>, IDAL.ILeaveMsgDAL
    {
	    public override void SetDbContext()
		{
			 dbContext = new BlogDb4ZynEntities();
			 dbContext.Configuration.ValidateOnSaveEnabled = false;
		}
    }

	public partial class PalLinkDAL : BaseDAL<ZYN.BLOG.Model.PalLink>, IDAL.IPalLinkDAL
    {
	    public override void SetDbContext()
		{
			 dbContext = new BlogDb4ZynEntities();
			 dbContext.Configuration.ValidateOnSaveEnabled = false;
		}
    }

	public partial class SearchDetailDAL : BaseDAL<ZYN.BLOG.Model.SearchDetail>, IDAL.ISearchDetailDAL
    {
	    public override void SetDbContext()
		{
			 dbContext = new BlogDb4ZynEntities();
			 dbContext.Configuration.ValidateOnSaveEnabled = false;
		}
    }

	public partial class SearchRankDAL : BaseDAL<ZYN.BLOG.Model.SearchRank>, IDAL.ISearchRankDAL
    {
	    public override void SetDbContext()
		{
			 dbContext = new BlogDb4ZynEntities();
			 dbContext.Configuration.ValidateOnSaveEnabled = false;
		}
    }

	public partial class StaticFileDAL : BaseDAL<ZYN.BLOG.Model.StaticFile>, IDAL.IStaticFileDAL
    {
	    public override void SetDbContext()
		{
			 dbContext = new BlogDb4ZynEntities();
			 dbContext.Configuration.ValidateOnSaveEnabled = false;
		}
    }

	public partial class VisitorDAL : BaseDAL<ZYN.BLOG.Model.Visitor>, IDAL.IVisitorDAL
    {
	    public override void SetDbContext()
		{
			 dbContext = new BlogDb4ZynEntities();
			 dbContext.Configuration.ValidateOnSaveEnabled = false;
		}
    }

	public partial class WebSettingDAL : BaseDAL<ZYN.BLOG.Model.WebSetting>, IDAL.IWebSettingDAL
    {
	    public override void SetDbContext()
		{
			 dbContext = new BlogDb4ZynEntities();
			 dbContext.Configuration.ValidateOnSaveEnabled = false;
		}
    }


}