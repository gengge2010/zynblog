
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYN.BLOG.IDAL;

namespace ZYN.BLOG.DAL
{
    /// <summary>
    /// DbSession：本质就是一个简单工厂+一个SaveChange方法，从抽象意义来说，它就是整个数据库访问层的统一入口
    /// 因为拿到了DbSession就能拿到整个数据库访问层所有的Dal。之前是：		
	/// public IUserInfoDal UserInfoDal
    ///    {
    ///        get { return new UserInfoDal(); }
    ///    }
	/// 现在是:私有的字段，公共属性。get中先判断有没有当前对象
	/// 因为当new一个对象时，是先初始化其字段值，再执行构造函数？
	/// 最后将Dal以接口的形式返回
    /// </summary>
	public partial class DbSession : IDAL.IDbSession
    {
		#region 01 数据接口 ArticleDAL
		private IArticleDAL _ArticleDAL;
		public IArticleDAL ArticleDAL
		{
			get
			{
				if(_ArticleDAL == null)
					_ArticleDAL = new ArticleDAL();
				return _ArticleDAL;
			}
			set
			{
				_ArticleDAL = value;
			}
		}
		#endregion

		#region 02 数据接口 CategoryDAL
		private ICategoryDAL _CategoryDAL;
		public ICategoryDAL CategoryDAL
		{
			get
			{
				if(_CategoryDAL == null)
					_CategoryDAL = new CategoryDAL();
				return _CategoryDAL;
			}
			set
			{
				_CategoryDAL = value;
			}
		}
		#endregion

		#region 03 数据接口 CommentDAL
		private ICommentDAL _CommentDAL;
		public ICommentDAL CommentDAL
		{
			get
			{
				if(_CommentDAL == null)
					_CommentDAL = new CommentDAL();
				return _CommentDAL;
			}
			set
			{
				_CommentDAL = value;
			}
		}
		#endregion

		#region 04 数据接口 HeadIconDAL
		private IHeadIconDAL _HeadIconDAL;
		public IHeadIconDAL HeadIconDAL
		{
			get
			{
				if(_HeadIconDAL == null)
					_HeadIconDAL = new HeadIconDAL();
				return _HeadIconDAL;
			}
			set
			{
				_HeadIconDAL = value;
			}
		}
		#endregion

		#region 05 数据接口 LeaveMsgDAL
		private ILeaveMsgDAL _LeaveMsgDAL;
		public ILeaveMsgDAL LeaveMsgDAL
		{
			get
			{
				if(_LeaveMsgDAL == null)
					_LeaveMsgDAL = new LeaveMsgDAL();
				return _LeaveMsgDAL;
			}
			set
			{
				_LeaveMsgDAL = value;
			}
		}
		#endregion

		#region 06 数据接口 PalLinkDAL
		private IPalLinkDAL _PalLinkDAL;
		public IPalLinkDAL PalLinkDAL
		{
			get
			{
				if(_PalLinkDAL == null)
					_PalLinkDAL = new PalLinkDAL();
				return _PalLinkDAL;
			}
			set
			{
				_PalLinkDAL = value;
			}
		}
		#endregion

		#region 07 数据接口 SearchDetailDAL
		private ISearchDetailDAL _SearchDetailDAL;
		public ISearchDetailDAL SearchDetailDAL
		{
			get
			{
				if(_SearchDetailDAL == null)
					_SearchDetailDAL = new SearchDetailDAL();
				return _SearchDetailDAL;
			}
			set
			{
				_SearchDetailDAL = value;
			}
		}
		#endregion

		#region 08 数据接口 SearchRankDAL
		private ISearchRankDAL _SearchRankDAL;
		public ISearchRankDAL SearchRankDAL
		{
			get
			{
				if(_SearchRankDAL == null)
					_SearchRankDAL = new SearchRankDAL();
				return _SearchRankDAL;
			}
			set
			{
				_SearchRankDAL = value;
			}
		}
		#endregion

		#region 09 数据接口 StaticFileDAL
		private IStaticFileDAL _StaticFileDAL;
		public IStaticFileDAL StaticFileDAL
		{
			get
			{
				if(_StaticFileDAL == null)
					_StaticFileDAL = new StaticFileDAL();
				return _StaticFileDAL;
			}
			set
			{
				_StaticFileDAL = value;
			}
		}
		#endregion

		#region 10 数据接口 VisitorDAL
		private IVisitorDAL _VisitorDAL;
		public IVisitorDAL VisitorDAL
		{
			get
			{
				if(_VisitorDAL == null)
					_VisitorDAL = new VisitorDAL();
				return _VisitorDAL;
			}
			set
			{
				_VisitorDAL = value;
			}
		}
		#endregion

		#region 11 数据接口 WebSettingDAL
		private IWebSettingDAL _WebSettingDAL;
		public IWebSettingDAL WebSettingDAL
		{
			get
			{
				if(_WebSettingDAL == null)
					_WebSettingDAL = new WebSettingDAL();
				return _WebSettingDAL;
			}
			set
			{
				_WebSettingDAL = value;
			}
		}
		#endregion

    }

}