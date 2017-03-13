
 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.BLL
{
    /// <summary>
    /// 各子业务层需要实现自己的I<>BLL，同时继承BaseBLL以便拥有各子BLL共有的CURD
    /// </summary>
	public partial class ArticleService : BaseService<ZYN.BLOG.Model.Article>,  IBLL.IArticleService
    {
		public override void SetDAL()
		{
			CurrentDAL = CurrentDbSession.ArticleDAL;
		}
    }

	public partial class CategoryService : BaseService<ZYN.BLOG.Model.Category>,  IBLL.ICategoryService
    {
		public override void SetDAL()
		{
			CurrentDAL = CurrentDbSession.CategoryDAL;
		}
    }

	public partial class CommentService : BaseService<ZYN.BLOG.Model.Comment>,  IBLL.ICommentService
    {
		public override void SetDAL()
		{
			CurrentDAL = CurrentDbSession.CommentDAL;
		}
    }

	public partial class HeadIconService : BaseService<ZYN.BLOG.Model.HeadIcon>,  IBLL.IHeadIconService
    {
		public override void SetDAL()
		{
			CurrentDAL = CurrentDbSession.HeadIconDAL;
		}
    }

	public partial class LeaveMsgService : BaseService<ZYN.BLOG.Model.LeaveMsg>,  IBLL.ILeaveMsgService
    {
		public override void SetDAL()
		{
			CurrentDAL = CurrentDbSession.LeaveMsgDAL;
		}
    }

	public partial class PalLinkService : BaseService<ZYN.BLOG.Model.PalLink>,  IBLL.IPalLinkService
    {
		public override void SetDAL()
		{
			CurrentDAL = CurrentDbSession.PalLinkDAL;
		}
    }

	public partial class SearchDetailService : BaseService<ZYN.BLOG.Model.SearchDetail>,  IBLL.ISearchDetailService
    {
		public override void SetDAL()
		{
			CurrentDAL = CurrentDbSession.SearchDetailDAL;
		}
    }

	public partial class SearchRankService : BaseService<ZYN.BLOG.Model.SearchRank>,  IBLL.ISearchRankService
    {
		public override void SetDAL()
		{
			CurrentDAL = CurrentDbSession.SearchRankDAL;
		}
    }

	public partial class StaticFileService : BaseService<ZYN.BLOG.Model.StaticFile>,  IBLL.IStaticFileService
    {
		public override void SetDAL()
		{
			CurrentDAL = CurrentDbSession.StaticFileDAL;
		}
    }

	public partial class VisitorService : BaseService<ZYN.BLOG.Model.Visitor>,  IBLL.IVisitorService
    {
		public override void SetDAL()
		{
			CurrentDAL = CurrentDbSession.VisitorDAL;
		}
    }

	public partial class WebSettingService : BaseService<ZYN.BLOG.Model.WebSetting>,  IBLL.IWebSettingService
    {
		public override void SetDAL()
		{
			CurrentDAL = CurrentDbSession.WebSettingDAL;
		}
    }


}