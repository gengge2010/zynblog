using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZYN.BLOG.Model;

namespace ZYN.BLOG.SiteSearch
{
    //QuartzJob要实现IJob接口，里面定义要定时执行的方法
    public class QuartzJob : IJob
    {
        IBLL.ISearchRankService hotSearchService = WebHelper.OperateHelper.Current.serviceSession.SearchRankService;
        IBLL.ISearchDetailService detailService = WebHelper.OperateHelper.Current.serviceSession.SearchDetailService;

        public void Execute(JobExecutionContext context)
        {
            hotSearchService.DeleteBy(h => true);
            var detailList= detailService.GetDataListBy(s => true);

            //先按keyword归类，然后select取出keyword值和各个值出现的次数
            var list = from d in detailList
                       group d by d.KeyWord into s
                       select new 
                       {
                           Id = System.Guid.NewGuid(),
                           Keyword = s.Key,
                           SearchTimes = s.Count()
                       };

            //统计后插入搜索热词排序表
            foreach (var item in list)
            {
                SearchRank rank = new SearchRank()
                {
                    Id = item.Id,
                    KeyWord = item.Keyword,
                    SearchTimes = item.SearchTimes
                };

                hotSearchService.Add(rank);
            }
        }
    }
}
