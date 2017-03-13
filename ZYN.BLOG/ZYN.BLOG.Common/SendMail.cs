using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZYN.BLOG.Common
{
    public class SendMail
    {
        #region 1.0 发送邮件方法 只需填写 收件人、内容 ，根据默认的SMTP地址 帐号 密码 直接发送
        /// <summary>
        /// 发送邮件方法 只需填写 收件人 内容 ， 根据默认的SMTP地址 帐号 密码 直接发送
        /// </summary>
        /// <param name="strto">收件人帐号</param>
        /// <param name="strSubject">主题</param>
        /// <param name="strBody">内容</param>
        /// <return>登录结果</return>
        public static void SendEMail(string strTo, string strSubject, string strBody)
        {
            SendSMTPEMail("smtp.126.com", "zynblog@126.com", "sss", strTo, strSubject, strBody);
        }
        #endregion

        #region 2.0 发送邮件方法 完整填写SMTP地址 帐号 密码 收件人 主题 内容 发送
        /// <summary>
        /// 发送邮件方法 只需填写 收件人 内容 ， 根据默认的SMTP地址 帐号 密码 直接发送
        /// </summary>
        public static void SendEMail(string strSmtpService, string strFrom, string strFromPass, string strto, string strSubject, string strBody)
        {
            SendSMTPEMail(strSmtpService, strFrom, strFromPass, strto, strSubject, strBody);
        }
        #endregion

        #region 发送邮件方法 需要SMTP地址 帐号 密码 收件人 主题 内容 发送
        /// <summary>
        /// 发送邮件方法 需要SMTP地址 帐号 密码 收件人 主题 内容 发送
        /// </summary>
        /// <param name="strSmtpServer">SMTP服务器</param>
        /// <paClass1ram name="strFrom">发件人的帐号</param>
        /// <param name="strFromPass">发件人密码</param>
        /// <param name="strto">收件人帐号</param>
        /// <param name="strSubject">主题</param>
        /// <param name="strBody">内容</param>
        private static void SendSMTPEMail(string strSmtpServer, string strFrom, string strFromPass, string strTo, string strSubject, string strBody)
        {
            try
            {
                //指定服务器的地址
                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(strSmtpServer);

                client.UseDefaultCredentials = false;
                //发送者邮箱/密码
                client.Credentials = new System.Net.NetworkCredential(strFrom, strFromPass);

                client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;

                System.Net.Mail.MailMessage mailMsg = new System.Net.Mail.MailMessage();
                mailMsg.From = new System.Net.Mail.MailAddress(strFrom, "zynblog系统通知");
                mailMsg.To.Add(strTo);
                mailMsg.Subject = strSubject;
                mailMsg.Body = strBody;

                mailMsg.BodyEncoding = System.Text.Encoding.UTF8;
                //允许有HTML
                mailMsg.IsBodyHtml = true;

                client.Send(mailMsg);
            }
            catch (Exception e)
            {
                string str = e.Message;
            }
        }
        #endregion
    }
}
