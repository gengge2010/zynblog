using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using ZYN.BLOG.Model;
using System.Web.SessionState;

/// <summary>
/// UploadHandler 的摘要说明
/// 因为本UEditor只是用在博文编辑和新增中的，故里面的路径设计完全是迎合文章目录的
/// 不考虑头像和论文文件的上传路径
/// </summary>
public class UploadHandler : Handler
{

    public UploadConfig UploadConfig { get; private set; }
    public UploadResult Result { get; private set; }

    public UploadHandler(HttpContext context, UploadConfig config)
        : base(context)
    {
        this.UploadConfig = config;
        this.Result = new UploadResult() { State = UploadState.Unknown };
    }

    public override void Process()
    {
        byte[] uploadFileBytes = null;
        //文件原名
        string uploadFileName = null;

        if (UploadConfig.Base64)
        {
            uploadFileName = UploadConfig.Base64Filename;
            uploadFileBytes = Convert.FromBase64String(Request[UploadConfig.UploadFieldName]);
        }
        else
        {
            //提交**的表单名称UploadConfig.UploadFieldName
            var file = Request.Files[UploadConfig.UploadFieldName];//json中 Config.GetString("imageFieldName")
            uploadFileName = file.FileName;

            //检查文件类型限制
            if (!CheckFileType(uploadFileName))
            {
                Result.State = UploadState.TypeNotAllow;
                WriteResult();
                return;
            }
            //检查文件大小限制
            if (!CheckFileSize(file.ContentLength))
            {
                Result.State = UploadState.SizeLimitExceed;
                WriteResult();
                return;
            }

            uploadFileBytes = new byte[file.ContentLength];
            try
            {
                //将文件流写入uploadFileBytes中
                file.InputStream.Read(uploadFileBytes, 0, file.ContentLength);
            }
            catch (Exception)
            {
                Result.State = UploadState.NetworkError;
                WriteResult();
            }
        }

        Result.OriginFileName = uploadFileName;

        //文件保存路径"imagePathFormat": "/Content/upfile/{yyyy}{mm}{dd}/{time}{rand:6}", 
        string savePath = PathFormatter.Format(uploadFileName, UploadConfig.PathFormat);
        //转换为绝对路径
        string localPath = Server.MapPath(savePath);
        try
        {
            if (!Directory.Exists(Path.GetDirectoryName(localPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(localPath));
            }
            //1.0 将文件保存到本地路径中
            File.WriteAllBytes(localPath, uploadFileBytes);

            //2.0 再上传至七牛，之后就一直用七牛的地址
            string bucket = System.Configuration.ConfigurationManager.AppSettings["QiNiuBucket"];
            string qiniuUrlIndex = System.Configuration.ConfigurationManager.AppSettings["QiNiuURL"];
            string key = UploadConfig.QiniuPathFormat + DateTime.Now.ToString("yyyyMMdd") + "/" + localPath.Split('\\').Last();

            bool qiniuFlag = ZYN.BLOG.WebHelper.QiniuHelper.UploadFile(bucket, key, localPath);
            if (qiniuFlag)
            {
                //3.0 为了节省服务器存储空间，保存至七牛后，将服务器文件删除。
                string dir = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "Content\\upfile";
                Directory.Delete(dir, true);

                //4.0 先将新上传的文章文件(图片和文件等)入库->（便于页面中链接的展示）->保存完或更新完文章后补全文件的FromArticleId
                #region 自定义： 开始把上传的图片信息临时存入Session;用于保存文章时获取文章内文件信息

                StaticFile artFile = new StaticFile()
                {
                    FileRawName = uploadFileName,
                    FileNowName = localPath.Split('\\').Last(),
                    //FileLocalPath = null,
                    FileOnLineURL = qiniuUrlIndex + "/" + key,   //保存到库中的均为真实路径
                    //FromArticleId = null, //添加完文章之后再赋值
                    FileType = 1,
                    UploadTime = DateTime.Now
                };

                ZYN.BLOG.IBLL.IStaticFileService fileService = ZYN.BLOG.WebHelper.OperateHelper.Current.serviceSession.StaticFileService;

                int val = fileService.Add(artFile); //先将其入库，拿到Id再补全FromArticleId
                if (val == 1)
                {
                    HttpContext context = HttpContext.Current;
                    HttpSessionState session = context.Session;

                    List<StaticFile> filelist = session["ArticleFiles"] as List<StaticFile>;
                    if (filelist == null)
                    {
                        filelist = new List<StaticFile>();
                        session["ArticleFiles"] = filelist;
                    }

                    filelist.Add(artFile);
                }

                //如果动作是上传图片，就在页面中直接显示，如果动作是上传文件，就给Controller下载路由（不行，文章保存后，才保存的文件。那能不能先保存文件到数据库，后期再把这些文件的FromArticleId补上,我看行）
                if (UploadConfig.ActionName == "uploadimage")
                    Result.Url = qiniuUrlIndex + "/" + key;
                else if (UploadConfig.ActionName == "uploadfile")
                {
                    if (val == 1)
                    {
                        //为了防止图标显示不正确，配合ueditor.all.js中的文件图标函数getFileIcon 加上.pdf
                        Result.Url = "http://www.zynblog.com/Admin/FileHandler/DownLoadFile?fileId=" + artFile.Id;
                        Result.Url += "&fileName=" + uploadFileName;
                    }
                    else
                        Result.Url = qiniuUrlIndex + "/" + key;
                }

                Result.State = UploadState.Success;
                #endregion
            }
            else
            {
                Result.Url = "";
                Result.State = UploadState.NetworkError;
                Result.ErrorMessage = "上传至本地成功,但是上传至七牛失败(可能是没有网络)";

                string dir = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "Content\\upfile";
                Directory.Delete(dir, true);
            }
        }
        catch (Exception e)
        {
            Result.State = UploadState.FileAccessError;
            Result.ErrorMessage = e.Message;
        }
        finally
        {
            WriteResult();
        }
    }

    private void WriteResult()
    {
        this.WriteJson(new
        {
            state = GetStateMessage(Result.State),
            url = Result.Url,
            title = Result.OriginFileName,
            original = Result.OriginFileName,
            error = Result.ErrorMessage
        });
    }

    private string GetStateMessage(UploadState state)
    {
        switch (state)
        {
            case UploadState.Success:
                return "SUCCESS";
            case UploadState.FileAccessError:
                return "文件访问出错，请检查写入权限";
            case UploadState.SizeLimitExceed:
                return "文件大小超出服务器限制";
            case UploadState.TypeNotAllow:
                return "不允许的文件格式";
            case UploadState.NetworkError:
                return "网络错误";
        }
        return "未知错误";
    }

    private bool CheckFileType(string filename)
    {
        var fileExtension = Path.GetExtension(filename).ToLower();
        return UploadConfig.AllowExtensions.Select(x => x.ToLower()).Contains(fileExtension);
    }

    private bool CheckFileSize(int size)
    {
        return size < UploadConfig.SizeLimit;
    }
}

public class UploadConfig
{
    public string ActionName { get; set; }
    /// <summary>
    /// 七牛的目录前缀
    /// </summary>
    public string QiniuPathFormat { get; set; }

    /// <summary>
    /// 文件命名规则
    /// </summary>
    public string PathFormat { get; set; }

    /// <summary>
    /// 上传表单域名称
    /// </summary>
    public string UploadFieldName { get; set; }

    /// <summary>
    /// 上传大小限制
    /// </summary>
    public int SizeLimit { get; set; }

    /// <summary>
    /// 上传允许的文件格式
    /// </summary>
    public string[] AllowExtensions { get; set; }

    /// <summary>
    /// 文件是否以 Base64 的形式上传
    /// </summary>
    public bool Base64 { get; set; }

    /// <summary>
    /// Base64 字符串所表示的文件名
    /// </summary>
    public string Base64Filename { get; set; }
}

public class UploadResult
{
    public UploadState State { get; set; }
    public string Url { get; set; }
    public string OriginFileName { get; set; }

    public string ErrorMessage { get; set; }
}

/// <summary>
/// 上传状态枚举
/// </summary>
public enum UploadState
{
    Success = 0,
    SizeLimitExceed = -1,
    TypeNotAllow = -2,
    FileAccessError = -3,
    NetworkError = -4,
    Unknown = 1,
}
