using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Qiniu.Conf;
using Qiniu.RS;
using Qiniu.IO;
using ZYN.BLOG.Model;
using System.IO;
using Qiniu.RPC;

namespace ZYN.BLOG.WebHelper
{
    public class QiniuHelper
    {
        public static IBLL.IWebSettingService settingService = WebHelper.OperateHelper.Current.serviceSession.WebSettingService;

        /// <summary>
        /// 网站启动时即配置七牛Key：用户名+密码(从数据库中取)
        /// </summary>
        public static void SetKey()
        {
            WebSetting setAccess = settingService.GetDataListBy(s => s.ConfigKey == "QiNiuACCESS_KEY")[0];
            WebSetting setSecret = settingService.GetDataListBy(s => s.ConfigKey == "QiNiuSECRET_KEY")[0];

            Qiniu.Conf.Config.ACCESS_KEY = setAccess.ConfigValue;
            Qiniu.Conf.Config.SECRET_KEY = setSecret.ConfigValue;
        }

        /// <summary>
        /// 上传图片到七牛空间
        /// </summary>
        /// <param name="bucket">设置上传的空间 "zynblog"</param>
        /// <param name="key">目标资源的最终名字."headicon/20160601/55c48c00-cc9e-42e4-8ad1-610d640b099f.jpg"</param>
        /// <param name="filePath">上传文件的本地路径</param>
        public static bool UploadFile(string bucket, string key, string filePath)
        {
            IOClient target = new IOClient();
            PutExtra extra = new PutExtra();

            //普通上传,只需要设置上传的空间名就可以了,第二个参数可以设定token过期时间
            PutPolicy put = new PutPolicy(bucket, 3600);

            //调用Token()方法生成上传的Token
            string upToken = put.Token();

            //调用PutFile()方法上传
            PutRet ret = target.PutFile(upToken, key, filePath, extra);

            //如果资源上传成功，服务端会响应HTTP 200返回码，且在响应内容中包含两个字段：
            //实验表明：在断网情况下，状态码依然为200！！！！
            //System.Net.HttpStatusCode code = ret.StatusCode; //OK:200
            //bool flag = false;
            //if (code == System.Net.HttpStatusCode.OK)
            //    flag = true;
            //return flag;

            bool flag = false;
            if (ret.key != null && ret.Hash != null)
                flag = true;

            return flag;
        }

        //key是去除前缀@(http://o82pwjziv.bkt.clouddn.com/)的;
        /// <summary>
        /// 2.0 删除文件
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DeleteFile(string bucket, string key)
        {
            //*****http://o82pwjziv.bkt.clouddn.com/articleImg/20160602/63600468179

            string qiniuUrlIndex = ConfigurationManager.AppSettings["QiNiuURL"];//http://o82pwjziv.bkt.clouddn.com/

            key = key.Replace(qiniuUrlIndex, "");

            RSClient client = new RSClient();
            CallRet ret = client.Delete(new EntryPath(bucket, key));
            if (ret.OK)
                return true;
            else
                return false;
        }
    }
}