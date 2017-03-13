using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;

namespace ZYN.BLOG.SiteSearch
{
    /// <summary>
    /// 分词
    /// </summary>
    public class SplitContent
    {
        /// <summary>
        /// 首先 对用户输入的搜索条件进行分词、根据分词结果去查找索引、得到结果、按相关度排序、反馈结果。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string[] SplitWords(string str)
        {
            List<string> list = new List<string>();

            Analyzer analyzer = new PanGuAnalyzer();//指定盘古分词
            TokenStream tokenStream = analyzer.TokenStream("", new StringReader(str));//参考文档就为""

            Lucene.Net.Analysis.Token token = null;
            while ((token = tokenStream.Next()) != null)
            {
                list.Add(token.TermText());
            }
            return list.ToArray();
        }
    }
}
