using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PanGu;

namespace ZYN.BLOG.SiteSearch
{
    /// <summary>
    /// 高亮显示
    /// </summary>
    public class HighlightShow
    {
        /// <summary>
        /// 对搜索的关键词高亮显示
        /// </summary>
        /// <param name="keyword">搜索的关键词项</param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string Highlight(string keyword, string content)
        {
            //创建HTMLFormatter,参数为高亮单词的前后缀 
            PanGu.HighLight.SimpleHTMLFormatter simpleHTMLFormatter =
                   new PanGu.HighLight.SimpleHTMLFormatter("<a href='#' class='highlightshow'>", "</a>");

            //创建 Highlighter ，输入HTMLFormatter 和 盘古分词对象Semgent 
            PanGu.HighLight.Highlighter highlighter =
                   new PanGu.HighLight.Highlighter(simpleHTMLFormatter, new Segment());

            //设置每个摘要段的字符数，暂时取300个字符吧
            highlighter.FragmentSize = 300;

            //获取最匹配的摘要段 
            return highlighter.GetBestFragment(keyword, content);
        }
    }
}
