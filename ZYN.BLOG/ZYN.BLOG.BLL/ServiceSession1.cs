
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYN.BLOG.IBLL;

namespace ZYN.BLOG.BLL
{
	public partial class ServiceSession : ZYN.BLOG.IBLL.IServiceSession
    {
		#region 01 业务接口 IArticleService (实际为类 依赖接口)
		IArticleService _ArticleService;
		public IArticleService ArticleService
		{
			get
			{
				if(_ArticleService == null)
					_ArticleService = new ArticleService();
				return _ArticleService;
			}
			set
			{
				_ArticleService = value;
			}
		}
		#endregion

		#region 02 业务接口 ICategoryService (实际为类 依赖接口)
		ICategoryService _CategoryService;
		public ICategoryService CategoryService
		{
			get
			{
				if(_CategoryService == null)
					_CategoryService = new CategoryService();
				return _CategoryService;
			}
			set
			{
				_CategoryService = value;
			}
		}
		#endregion

		#region 03 业务接口 ICommentService (实际为类 依赖接口)
		ICommentService _CommentService;
		public ICommentService CommentService
		{
			get
			{
				if(_CommentService == null)
					_CommentService = new CommentService();
				return _CommentService;
			}
			set
			{
				_CommentService = value;
			}
		}
		#endregion

		#region 04 业务接口 IHeadIconService (实际为类 依赖接口)
		IHeadIconService _HeadIconService;
		public IHeadIconService HeadIconService
		{
			get
			{
				if(_HeadIconService == null)
					_HeadIconService = new HeadIconService();
				return _HeadIconService;
			}
			set
			{
				_HeadIconService = value;
			}
		}
		#endregion

		#region 05 业务接口 ILeaveMsgService (实际为类 依赖接口)
		ILeaveMsgService _LeaveMsgService;
		public ILeaveMsgService LeaveMsgService
		{
			get
			{
				if(_LeaveMsgService == null)
					_LeaveMsgService = new LeaveMsgService();
				return _LeaveMsgService;
			}
			set
			{
				_LeaveMsgService = value;
			}
		}
		#endregion

		#region 06 业务接口 IPalLinkService (实际为类 依赖接口)
		IPalLinkService _PalLinkService;
		public IPalLinkService PalLinkService
		{
			get
			{
				if(_PalLinkService == null)
					_PalLinkService = new PalLinkService();
				return _PalLinkService;
			}
			set
			{
				_PalLinkService = value;
			}
		}
		#endregion

		#region 07 业务接口 ISearchDetailService (实际为类 依赖接口)
		ISearchDetailService _SearchDetailService;
		public ISearchDetailService SearchDetailService
		{
			get
			{
				if(_SearchDetailService == null)
					_SearchDetailService = new SearchDetailService();
				return _SearchDetailService;
			}
			set
			{
				_SearchDetailService = value;
			}
		}
		#endregion

		#region 08 业务接口 ISearchRankService (实际为类 依赖接口)
		ISearchRankService _SearchRankService;
		public ISearchRankService SearchRankService
		{
			get
			{
				if(_SearchRankService == null)
					_SearchRankService = new SearchRankService();
				return _SearchRankService;
			}
			set
			{
				_SearchRankService = value;
			}
		}
		#endregion

		#region 09 业务接口 IStaticFileService (实际为类 依赖接口)
		IStaticFileService _StaticFileService;
		public IStaticFileService StaticFileService
		{
			get
			{
				if(_StaticFileService == null)
					_StaticFileService = new StaticFileService();
				return _StaticFileService;
			}
			set
			{
				_StaticFileService = value;
			}
		}
		#endregion

		#region 10 业务接口 IVisitorService (实际为类 依赖接口)
		IVisitorService _VisitorService;
		public IVisitorService VisitorService
		{
			get
			{
				if(_VisitorService == null)
					_VisitorService = new VisitorService();
				return _VisitorService;
			}
			set
			{
				_VisitorService = value;
			}
		}
		#endregion

		#region 11 业务接口 IWebSettingService (实际为类 依赖接口)
		IWebSettingService _WebSettingService;
		public IWebSettingService WebSettingService
		{
			get
			{
				if(_WebSettingService == null)
					_WebSettingService = new WebSettingService();
				return _WebSettingService;
			}
			set
			{
				_WebSettingService = value;
			}
		}
		#endregion

    }

}