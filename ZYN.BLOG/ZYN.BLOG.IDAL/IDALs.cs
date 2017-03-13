
 
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.IDAL
{
    /// <summary>
    /// 自动化生成各子IDAL的定义
    /// </summary>
	public partial interface IArticleDAL : IBaseDAL<ZYN.BLOG.Model.Article>
    {
    }

	public partial interface ICategoryDAL : IBaseDAL<ZYN.BLOG.Model.Category>
    {
    }

	public partial interface ICommentDAL : IBaseDAL<ZYN.BLOG.Model.Comment>
    {
    }

	public partial interface IHeadIconDAL : IBaseDAL<ZYN.BLOG.Model.HeadIcon>
    {
    }

	public partial interface ILeaveMsgDAL : IBaseDAL<ZYN.BLOG.Model.LeaveMsg>
    {
    }

	public partial interface IPalLinkDAL : IBaseDAL<ZYN.BLOG.Model.PalLink>
    {
    }

	public partial interface ISearchDetailDAL : IBaseDAL<ZYN.BLOG.Model.SearchDetail>
    {
    }

	public partial interface ISearchRankDAL : IBaseDAL<ZYN.BLOG.Model.SearchRank>
    {
    }

	public partial interface IStaticFileDAL : IBaseDAL<ZYN.BLOG.Model.StaticFile>
    {
    }

	public partial interface IVisitorDAL : IBaseDAL<ZYN.BLOG.Model.Visitor>
    {
    }

	public partial interface IWebSettingDAL : IBaseDAL<ZYN.BLOG.Model.WebSetting>
    {
    }


}