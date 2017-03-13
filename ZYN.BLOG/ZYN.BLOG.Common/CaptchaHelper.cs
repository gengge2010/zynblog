using System;
using System.Reflection;
using ZYN.BLOG.Common.Captcha;

namespace ZYN.BLOG.Common
{
    public class CaptchaHelper
    {
        /// <summary>
        /// 生成登录验证码
        /// </summary>
        public static void CreateCode()
        {
            string code;
            byte[] img = new CaptchaStyle2().CreateImage(out code);
            System.Web.HttpContext.Current.Session["Captcha"] = code;
            System.Web.HttpContext.Current.Response.ClearContent();
            System.Web.HttpContext.Current.Response.ContentType = "image/Gif";
            System.Web.HttpContext.Current.Response.BinaryWrite(img);
            System.Web.HttpContext.Current.Response.End();
        }

        public static byte[] CreateCodeImg()
        {
            string code;
            byte[] img = new CaptchaStyle2().CreateImage(out code);
            System.Web.HttpContext.Current.Session["Captcha"] = code;
            return img;
        }


        /// <summary>
        /// 判断验证码是否输入验证通过
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool Captcha(string code)
        {
            return Captcha(code, false);
        }

        /// <summary>
        /// 判断验证码是否输入验证通过
        /// </summary>
        /// <param name="code"></param>
        /// <param name="isRemove"></param>
        /// <returns></returns>
        public static bool Captcha(string code,bool isRemove)
        {
            var captcha = System.Web.HttpContext.Current.Session["Captcha"];
            if (isRemove)
            {
                System.Web.HttpContext.Current.Session.Remove("Captcha");
            }
            if (!string.IsNullOrEmpty(code) && code.Equals(captcha))
            {
                return true;
            }
            return false;
        }
    }
}
