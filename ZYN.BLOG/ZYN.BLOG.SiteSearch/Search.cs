using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using ZYN.BLOG.Model;

namespace ZYN.BLOG.SiteSearch
{
    public class Search
    {
        public static List<SearchResult> SearchContent(string keywords)
        {
            string indexPath = System.Configuration.ConfigurationManager.AppSettings["IndexPath"];
            keywords = keywords.ToLower();

            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NoLockFactory());
            IndexReader reader = IndexReader.Open(directory, true);
            IndexSearcher searcher = new IndexSearcher(reader);
            //搜索条件
            //PhraseQuery query = new PhraseQuery();
            ////将用户输入的搜索词语进行分词
            //foreach (string word in SplitContent.SplitWords(keywords))
            //{
            //    //Add关系
            //    query.Add(new Term("content", word)); //从term为"msg"的索引文件中找
            //}  
            //query.SetSlop(100); //多个查询条件的词之间的最大距离.在文章中相隔太远 也就无意义

            //query.Add(new Term("msg", keywords));

            //关键词Or关系设置
            BooleanQuery queryOr = new BooleanQuery();
            TermQuery queryterm = null;
            //将用户的搜索key分词
            string[] splitKeywords = SplitContent.SplitWords(keywords);

            foreach (string word in splitKeywords)
            {
                queryterm = new TermQuery(new Term("content", word));
                queryOr.Add(queryterm, BooleanClause.Occur.SHOULD);//这里设置 条件为Or关系
            }
            queryterm = new TermQuery(new Term("content", keywords));
            queryOr.Add(queryterm, BooleanClause.Occur.SHOULD);

            //TopScoreDocCollector是盛放查询结果的容器
            TopScoreDocCollector collector = TopScoreDocCollector.create(1000, true);
            searcher.Search(queryOr, null, collector);//根据query查询条件进行查询，查询结果放入collector容器
            ScoreDoc[] docs = collector.TopDocs(0, collector.GetTotalHits()).scoreDocs;//得到所有查询结果中的文档,GetTotalHits():表示总条数   TopDocs(300, 20);//表示得到300（从300开始），到320（结束）的文档内容.       //可以用来实现分页功能

            //将搜索到的结果保存到list中
            List<SearchResult> list = new List<SearchResult>();

            for (int i = 0; i < docs.Length; i++)
            {
                //搜索ScoreDoc[]只能获得文档的id,这样不会把查询结果的Document一次性加载到内存中。降低了内存压力，需要获得文档的详细内容的时候通过searcher.Doc来根据文档id来获得文档的详细内容对象Document.
                int docId = docs[i].doc;//得到查询结果文档的id（Lucene内部分配的id）

                Document doc = searcher.Doc(docId);//找到文档id对应的文档详细信息.

                SearchResult result = new SearchResult();
                result.Content = HighlightShow.Highlight(keywords, doc.Get("content"));
                result.Title = doc.Get("title");
                result.Id = Convert.ToInt32(doc.Get("id"));
                result.Url = "/Archives/Index/" + doc.Get("id");

                list.Add(result);
            }

            //无论是否有搜索结果,都将该用户的本次搜索内容 加入到数据库中
            //第一步：将用户搜索的内容添加到SearchDetail详情表
            Model.SearchDetail model = new Model.SearchDetail();
            model.Id = Guid.NewGuid();
            model.KeyWord = keywords;
            model.SearchDate = DateTime.Now.ToLocalTime();

            IBLL.ISearchDetailService searchService = WebHelper.OperateHelper.Current.serviceSession.SearchDetailService;

            int val = searchService.Add(model);

            return list;
        }
    }
}
