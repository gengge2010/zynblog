using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZYN.BLOG.jqDataTableModel;
using ZYN.BLOG.Model;

namespace ZYN.BLOG.Areas.Admin.Controllers
{
    public class AdminCategoryController : BaseController
    {
        IBLL.ICategoryService categoryService = WebHelper.OperateHelper.Current.serviceSession.CategoryService;
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 2.0 jquery.datatable()插件 Ajax Get类别列表
        /// </summary>
        /// <returns>datatable-JSON</returns>
        public JsonResult GetCategoriesJson(jqDataTableParameter tableParam)
        {
            //0.0 全部数据
            List<Category> DataSource = categoryService.GetDataListBy(a => true, a => a.SubTime);

            //1.0 首先获取datatable提交过来的参数
            string echo = tableParam.sEcho;  //用于客户端自己的校验
            int dataStart = tableParam.iDisplayStart;//要请求的该页第一条数据的序号
            int pageSize = tableParam.iDisplayLength == -1 ? DataSource.Count : tableParam.iDisplayLength;//每页容量（=-1表示取全部数据）
            string search = tableParam.sSearch;

            //2.0 根据参数(起始序号、每页容量、查询参数)查询数据
            if (!String.IsNullOrEmpty(search))
            {
                var data = DataSource.Where(c => c.Name.Contains(search) ||
                                 c.Descn.Contains(search))
                     .Skip<Category>(dataStart)
                     .Take(pageSize)
                     .Select(c => new
                     {
                         Id = c.Id,
                         Name = c.Name,
                         Descript = c.Descn,
                         SubTime = c.SubTime.ToString(),
                         Status = c.Status
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
            else
            {
                var data = DataSource.Skip<Category>(dataStart)
                                     .Take(pageSize)
                                     .Select(c => new
                                     {
                                         Id = c.Id,
                                         Name = c.Name,
                                         Descript = c.Descn,
                                         SubTime = c.SubTime.ToString(),
                                         Status = c.Status
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
        }

        /// <summary>
        /// 3.0 根据id(软删除)分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteCategory(int id)
        {
            Category category = new Category()
            {
                Id = id,
                Status = 0
            };

            int val = categoryService.Update(category, "Status");

            return Json(val);
        }


        /// <summary>
        /// 4.0 根据id将分类设为启用状态
        /// </summary>
        [HttpPost]
        public ActionResult PublicCategory(int id)
        {
            Category category = new Category()
            {
                Id = id,
                Status = 1
            };
            int val = categoryService.Update(category, "Status");

            return Json(val);
        }

        /// <summary>
        /// 5.1 ajax get 获取被修改的model
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetEditedEntity(int id)
        {
            Category entity = categoryService.GetEntity(id);

            return Json(new
            {
                Id = entity.Id,
                Name = entity.Name,
                Descn = entity.Descn,
                Status = entity.Status,
                SubTime = entity.SubTime.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 5.2 ajax post 更新被修改的model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetEditedEntity(Category model)
        {
            model.AltTime = DateTime.Now;
           int val= categoryService.Update(model, "Name", "Descn", "Status", "SubTime", "AltTime");

           return Json(val);
        }

        /// <summary>
        /// 6.0 新增分类
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddCategory(Category model)
        {
            model.SubTime = DateTime.Now;

            int val = categoryService.Add(model);

            return Json(val);
        }
    }
}
