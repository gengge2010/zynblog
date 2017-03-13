
 
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.IBLL
{
    /// <summary>
    /// 子业务接口I<>BLL 继承 IBaseBLL父接口
    /// </summary>
	public partial interface IArticleService : IBaseService<ZYN.BLOG.Model.Article>
    {
    }

	public partial interface ICategoryService : IBaseService<ZYN.BLOG.Model.Category>
    {
    }

	public partial interface ICommentService : IBaseService<ZYN.BLOG.Model.Comment>
    {
    }

	public partial interface IHeadIconService : IBaseService<ZYN.BLOG.Model.HeadIcon>
    {
    }

	public partial interface ILeaveMsgService : IBaseService<ZYN.BLOG.Model.LeaveMsg>
    {
    }

	public partial interface IPalLinkService : IBaseService<ZYN.BLOG.Model.PalLink>
    {
    }

	public partial interface ISearchDetailService : IBaseService<ZYN.BLOG.Model.SearchDetail>
    {
    }

	public partial interface ISearchRankService : IBaseService<ZYN.BLOG.Model.SearchRank>
    {
    }

	public partial interface IStaticFileService : IBaseService<ZYN.BLOG.Model.StaticFile>
    {
    }

	public partial interface IVisitorService : IBaseService<ZYN.BLOG.Model.Visitor>
    {
    }

	public partial interface IWebSettingService : IBaseService<ZYN.BLOG.Model.WebSetting>
    {
    }


}