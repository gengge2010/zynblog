using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ZYN.BLOG.Common
{
    public class StringHelper
    {
        /// <summary>
        /// 格式化字符串长度,超出部分显示省略号,区分汉字和字母。
        /// </summary>
        /// <param name="rawstr">要截取的字符串</param>
        /// <param name="n">截取长度,多出部分用...代替</param>
        /// <returns></returns>
        public static string StringCut(string rawstr, int n)
        {
            string temp = string.Empty;
            //如果原字符串长度比需要的长度n小,直接返回原字符串
            if (System.Text.Encoding.Default.GetByteCount(rawstr) <= n)
            {
                return rawstr;
            }
            else
            {
                int t = 0;
                char[] q = rawstr.ToCharArray();
                for (int i = 0; i < q.Length && t < n; i++)
                {
                    if ((int)q[i] >= 0x4E00 && (int)q[i] <= 0x9FA5)//是否汉字
                    {
                        temp += q[i];
                        t += 2;
                    }
                    else
                    {
                        temp += q[i];
                        t++;
                    }
                }
                return (temp + "...");
            }
        }

        /// <summary>
        /// 清除文本中Html的标签
        /// </summary>
        /// <param name="Content">原Html字符串</param>
        /// <returns>去除标签的字符串</returns>
        public static string ClearHtml(string Content)
        {
            Content = ReplaceHtml("&#[^>]*;", "", Content);
            Content = ReplaceHtml("</?marquee[^>]*>", "", Content);
            Content = ReplaceHtml("</?object[^>]*>", "", Content);
            Content = ReplaceHtml("</?param[^>]*>", "", Content);
            Content = ReplaceHtml("</?embed[^>]*>", "", Content);
            Content = ReplaceHtml("</?table[^>]*>", "", Content);
            Content = ReplaceHtml("&nbsp;", "", Content);
            Content = ReplaceHtml("</?tr[^>]*>", "", Content);
            Content = ReplaceHtml("</?th[^>]*>", "", Content);
            Content = ReplaceHtml("</?p[^>]*>", "", Content);
            Content = ReplaceHtml("</?a[^>]*>", "", Content);
            Content = ReplaceHtml("</?h[^>]*>", "", Content);
            Content = ReplaceHtml("</?img[^>]*>", "", Content);
            Content = ReplaceHtml("</?tbody[^>]*>", "", Content);
            Content = ReplaceHtml("</?ul[^>]*>", "", Content);
            Content = ReplaceHtml("</?ol[^>]*>", "", Content);
            Content = ReplaceHtml("</?li[^>]*>", "", Content);
            Content = ReplaceHtml("</?span[^>]*>", "", Content);
            Content = ReplaceHtml("</?div[^>]*>", "", Content);
            Content = ReplaceHtml("</?th[^>]*>", "", Content);
            Content = ReplaceHtml("</?td[^>]*>", "", Content);
            Content = ReplaceHtml("</?script[^>]*>", "", Content);
            Content = ReplaceHtml("(javascript|jscript|vbscript|vbs):", "", Content);
            Content = ReplaceHtml("on(mouse|exit|error|click|key)", "", Content);
            Content = ReplaceHtml("<\\?xml[^>]*>", "", Content);
            Content = ReplaceHtml("<\\/?[a-z]+:[^>]*>", "", Content);
            Content = ReplaceHtml("</?font[^>]*>", "", Content);
            Content = ReplaceHtml("</?b[^>]*>", "", Content);
            Content = ReplaceHtml("</?u[^>]*>", "", Content);
            Content = ReplaceHtml("</?i[^>]*>", "", Content);
            Content = ReplaceHtml("</?strong[^>]*>", "", Content);

            string clearHtml = Content;
            return clearHtml;
        }

        /// <summary>
        /// 清除文本中的Html标签
        /// </summary>
        /// <param name="patrn">要替换的标签正则表达式</param>
        /// <param name="strRep">替换为的内容</param>
        /// <param name="content">要替换的内容</param>
        /// <returns>去除标签的字符串</returns>
        public static string ReplaceHtml(string patrn, string strRep, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                content = "";
            }
            Regex rgEx = new Regex(patrn, RegexOptions.IgnoreCase);
            string strTxt = rgEx.Replace(content, strRep);
            return strTxt;
        }

        /// <summary>
        /// 另一种过滤html的方式(还没试)去除HTML标记
        /// </summary>
        /// <param name="Htmlstring">包括HTML的源码</param>
        /// <returns>已经去除后的文字</returns>
        public static string NoHTML(string Htmlstring)
        {
            if (string.IsNullOrEmpty(Htmlstring))
                return "";
            //删除脚本
            Htmlstring = Htmlstring.Replace("\r\n", "");
            Htmlstring = Regex.Replace(Htmlstring, @"<script.*?</script>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<style.*?</style>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<.*?>", "", RegexOptions.IgnoreCase);
            //删除HTML
            Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"-->", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);
            Htmlstring = Htmlstring.Replace("&ldquo;", "\"");
            Htmlstring = Htmlstring.Replace("&rdquo;", "\"");
            Htmlstring = Htmlstring.Replace("<", "");
            Htmlstring = Htmlstring.Replace(">", "");
            Htmlstring = Htmlstring.Replace("\r\n", "");
            Htmlstring = Htmlstring.Replace("&mdash;", "-");
            Htmlstring = Htmlstring.Replace("&middot;", "·");

            //Htmlstring = Server.HtmlEncode(Htmlstring).Trim();
            return Htmlstring;
        }

    }
}
