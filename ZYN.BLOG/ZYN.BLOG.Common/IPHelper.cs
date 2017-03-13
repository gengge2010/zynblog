using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Runtime.Remoting.Contexts;

namespace ZYN.BLOG.Common
{
    public class IPHelper
    {
        /// <summary>
        /// 获取PC名称
        /// </summary>
        /// <returns></returns>
        public static string IPHostName()
        {
            return System.Net.Dns.GetHostName();
        }
        /// <summary>
        /// 获得本机局域网IP地址
        /// </summary>
        /// <returns></returns>
        public static string IPAddress()
        {
            return Dns.GetHostAddresses(IPHostName())[0].ToString();
            //string ip = "";
            //if (Context.Request.ServerVariables["HTTP_VIA"] != null) 
            //{
            //  ip =Context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            //}
            //else
            //{
            //    ip = Context.Request.ServerVariables["REMOTE_ADDR"].ToString(); //While it can't get the Client IP, it will return proxy IP.
            //} 
        }

        public static string GetIP()  {
            string result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (null == result || result == String.Empty)
            {
                result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            if (null == result || result == String.Empty)
            {
                result = HttpContext.Current.Request.UserHostAddress;
            }
            return result;
       }
        /// <summary>
        /// 获得拨号动态分配IP地址
        /// </summary>
        /// <returns></returns>
        public static string IPDynamicAddress()
        {
            return Dns.GetHostAddresses(IPHostName())[0].ToString();
        }

         /// <summary>
         /// 得到当前完整主机头
         /// </summary>
         /// <returns></returns>
         public static string GetCurrentFullHost()
         {
             HttpRequest request = System.Web.HttpContext.Current.Request;
             if (!request.Url.IsDefaultPort)
                 return string.Format("{0}:{1}", request.Url.Host, request.Url.Port.ToString());

             return request.Url.Host;
         }

         /// <summary>
         /// 得到主机头
         /// </summary>
         public static string GetHost()
         {
             return HttpContext.Current.Request.Url.Host;
         }
    }
}
