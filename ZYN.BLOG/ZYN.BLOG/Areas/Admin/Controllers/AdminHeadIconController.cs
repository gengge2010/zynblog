using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZYN.BLOG.FormatModel;
using ZYN.BLOG.jqDataTableModel;
using ZYN.BLOG.Model;
using ZYN.BLOG.WebHelper;

namespace ZYN.BLOG.Areas.Admin.Controllers
{
    public class AdminHeadIconController : BaseController
    {
        IBLL.IHeadIconService iconService = WebHelper.OperateHelper.Current.serviceSession.HeadIconService;
        IBLL.IWebSettingService settingService = WebHelper.OperateHelper.Current.serviceSession.WebSettingService;

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 2.0 jquery.datatable()插件 Ajax Get头像列表
        /// </summary>
        /// <returns>datatable-JSON</returns>
        public JsonResult GetHeadiconsJson(jqDataTableParameter tableParam)
        {
            //0.0 全部数据
            List<HeadIcon> DataSource = iconService.GetDataListBy(h => true, h => h.UploadTime);

            //1.0 首先获取datatable提交过来的参数
            string echo = tableParam.sEcho;  //用于客户端自己的校验
            int dataStart = tableParam.iDisplayStart;//要请求的该页第一条数据的序号
            int pageSize = tableParam.iDisplayLength == -1 ? DataSource.Count : tableParam.iDisplayLength;//每页容量（=-1表示取全部数据）

            //2.0 根据参数(起始序号、每页容量)查询数据
            var data = DataSource.Skip<HeadIcon>(dataStart)
                                 .Take(pageSize)
                                 .Select(h => new
                                 {
                                     Id = h.Id,
                                     HeadImg = h.IconURL,
                                     RawName = h.IconRawName,
                                     NowName = h.IconName,
                                     HeadImgUrl = h.IconURL,
                                     UploadTime = h.UploadTime.ToString(),
                                     Status = h.Status
                                 }).ToList();

            //3.0 构造datatable所需要的数据json对象...aaData里面应是一个二维数组：即里面是一个数组[["","",""],[],[],[]]
            return Json(new
            {
                sEcho = echo,
                iTotalRecords = DataSource.Count(),
                iTotalDisplayRecords = DataSource.Count(),
                aaData = data
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 3.0 根据id(软删除)分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteHeadIcon(int id)
        {
            HeadIcon head = new HeadIcon()
            {
                Id = id,
                Status = 0
            };

            int val = iconService.Update(head, "Status");

            return Json(val);
        }


        /// <summary>
        /// 4.0 根据id将分类设为启用状态
        /// </summary>
        [HttpPost]
        public ActionResult PublicHeadIcon(int id)
        {
            HeadIcon head = new HeadIcon()
            {
                Id = id,
                Status = 1
            };

            int val = iconService.Update(head, "Status");

            return Json(val);
        }
    }
}
