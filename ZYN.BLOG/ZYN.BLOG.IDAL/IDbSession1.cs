
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.IDAL
{
	public partial interface IDbSession
    {
		IDAL.IArticleDAL ArticleDAL{get;set;}
		IDAL.ICategoryDAL CategoryDAL{get;set;}
		IDAL.ICommentDAL CommentDAL{get;set;}
		IDAL.IHeadIconDAL HeadIconDAL{get;set;}
		IDAL.ILeaveMsgDAL LeaveMsgDAL{get;set;}
		IDAL.IPalLinkDAL PalLinkDAL{get;set;}
		IDAL.ISearchDetailDAL SearchDetailDAL{get;set;}
		IDAL.ISearchRankDAL SearchRankDAL{get;set;}
		IDAL.IStaticFileDAL StaticFileDAL{get;set;}
		IDAL.IVisitorDAL VisitorDAL{get;set;}
		IDAL.IWebSettingDAL WebSettingDAL{get;set;}
    }

}