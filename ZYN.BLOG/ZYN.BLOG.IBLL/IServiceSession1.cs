
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.IBLL
{
	public partial interface IServiceSession
    {
		IArticleService ArticleService{get;set;}
		ICategoryService CategoryService{get;set;}
		ICommentService CommentService{get;set;}
		IHeadIconService HeadIconService{get;set;}
		ILeaveMsgService LeaveMsgService{get;set;}
		IPalLinkService PalLinkService{get;set;}
		ISearchDetailService SearchDetailService{get;set;}
		ISearchRankService SearchRankService{get;set;}
		IStaticFileService StaticFileService{get;set;}
		IVisitorService VisitorService{get;set;}
		IWebSettingService WebSettingService{get;set;}
    }

}