using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ZYN.BLOG.FormatModel;
using ZYN.BLOG.Model;
using ZYN.BLOG.WebHelper;

namespace ZYN.BLOG.Areas.Admin.Controllers
{
    public class FileHandlerController : Controller
    {
        IBLL.IHeadIconService iconService = WebHelper.OperateHelper.Current.serviceSession.HeadIconService;

        #region 1.0 上传头像图片到七牛云存储; [HttpPost] ActionResult UploadHeadIcon()

        /// <summary>
        /// 1.0 MVC 上传文件
        /// 上传头像图片到七牛云存储
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadHeadIcon()
        {
            string bucket = ConfigurationManager.AppSettings["QiNiuBucket"]; //七牛空间
            string localPath = "~/Content/images/tempicon/";  //本地文件绝对路径

            string qiniuPath = ConfigurationManager.AppSettings["PathOfHeadIcon"]; //七牛路径  "headicon/日期/"

            qiniuPath += "/" + DateTime.Now.ToString("yyyyMMdd") + "/"; //七牛路径 headicon/
            string fileType = ".jpg,.jpeg,.png,.gif";
            int maxSize = 5;

            //开始上传
            HttpPostedFileBase file = Request.Files[0];
            if (file != null)
            {
                List<string> extList = fileType.Split(',').ToList();
                var extension = Path.GetExtension(file.FileName); //后缀
                if (extension != null && extList.Contains(extension.ToLower()))
                {
                    string extendName = extension.ToLower();  //小写后缀名
                    int length = file.ContentLength;   //文件大小
                    if (length > maxSize * 1024 * 1024)
                    {
                        //上传失败 
                        string warning = "文件不得大于" + maxSize + "MB";

                        return Content("<script>window.parent.uploadSuccess('" + warning + "| ');</script>");
                    }
                    else
                    {

                        if (!Directory.Exists(localPath))   //创建目录
                        {
                            Directory.CreateDirectory(Server.MapPath(localPath));
                        }

                        localPath += file.FileName;
                        file.SaveAs(Server.MapPath(localPath));  //先 保存文件 到本地

                        string qiniufileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + extendName;
                        //上传至七牛
                        qiniuPath += qiniufileName;    //日期/文件名
                        bool uploadflag = QiniuHelper.UploadFile(bucket, qiniuPath, Server.MapPath(localPath));

                        if (uploadflag)
                        {
                            //先删除本地临时图片
                            System.IO.File.Delete(Server.MapPath(localPath));

                            string qiniuUrlIndex = ConfigurationManager.AppSettings["QiNiuURL"];

                            HeadIcon icon = new HeadIcon()
                            {
                                IconName = qiniufileName,  //现文件名
                                IconRawName = file.FileName, //原文件名 
                                IconURL = qiniuUrlIndex + "/" + qiniuPath,
                                Status = 1,
                                UploadTime = DateTime.Now
                            };

                            //入库
                            int val = iconService.Add(icon);
                            if (val == 1)
                            {
                                return Content("<script>window.parent.uploadSuccess('上传成功!|" + icon.IconURL + "');</script>");
                            }
                            else
                            {
                                return Content("<script>window.parent.uploadSuccess('上传至七牛成功,但入库时失败!|" + icon.IconURL + "');</script>");
                            }
                        }
                        else
                        {
                            string warning = "上传至七牛失败";
                            return Content("<script>window.parent.uploadSuccess('" + warning + "| ');</script>");
                        }
                    }
                }
                else
                {
                    string warning = "图片格式不对";
                    return Content("<script>window.parent.uploadSuccess('" + warning + "| ');</script>");
                }
            }
            else
            {
                string warning = "还没选择文件";
                return Content("<script>window.parent.uploadSuccess('" + warning + "| ');</script>");
            }
        } 
        #endregion

        #region 2.0 下载网络资源
        /// <summary>
        /// 2.0 文件下载
        /// </summary>
        /// <returns></returns>
        public FileResult DownLoadFile()
        {
            int fileId = Convert.ToInt32(Request["fileId"]);  //文件Id
            IBLL.IStaticFileService fileService = OperateHelper.Current.serviceSession.StaticFileService;
            StaticFile file = fileService.GetEntity(fileId);

            WebClient webClient = new WebClient();
            byte[] downStream = webClient.DownloadData(file.FileOnLineURL);

            return File(downStream, "application/octet-stream", file.FileRawName);
        } 
        #endregion
    }
}
