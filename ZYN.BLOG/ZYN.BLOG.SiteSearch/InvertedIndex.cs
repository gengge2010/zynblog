using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Documents;
using Lucene.Net.Analysis.PanGu;
using System.Web;

namespace ZYN.BLOG.SiteSearch
{
    /// <summary>
    /// 站内搜索：倒排索引
    /// </summary>
    public sealed class InvertedIndex
    {
        public static readonly InvertedIndex IndexManager = new InvertedIndex();
        public InvertedIndex()
        {

        }

        //队里中存的是生产者的"产品"：即要创建索引的内容
        private Queue<IndexTask> queue = new Queue<IndexTask>();

        //操作队列的一些列方法：
        /// <summary>
        /// 1.0 新增索引项.每新增一篇博客就调用一次Add()方法
        /// </summary>
        /// <param name="id">博客id</param>
        /// <param name="title">标题</param>
        /// <param name="msg">内容</param>
        public void Add(int id, string title, string content)
        {
            IndexTask task = new IndexTask()
            {
                Id = id,
                Title = title,
                Content = content,
                Type = TaskType.Add
            };

            //将该任务插入队列
            queue.Enqueue(task);
        }

        /// <summary>
        /// 2.0 删除索引项
        /// </summary>
        /// <param name="id"></param>
        public void Delete(int id)
        {
            IndexTask task = new IndexTask();
            task.Id = id;
            task.Type = TaskType.Delete;

            queue.Enqueue(task);
        }

        public void Update(int id, string title, string content)
        {
            IndexTask task = new IndexTask()
            {
                Id = id,
                Title = title,
                Content = content,
                Type = TaskType.Update
            };

            queue.Enqueue(task);
        }

        //定义一个线程，将队列中的数据取出来插入索引库中
        //网站一旦启动,就开启此线程，一直扫描等待接收"任务"(Application_Start())
        public void Start()
        {
            Thread myThread = new Thread(IndexThreadHandler);
            myThread.IsBackground = true;
            myThread.Start();
        }

        /// <summary>
        /// 索引线程处理
        /// </summary>
        public void IndexThreadHandler()
        {
            while (true)
            {
                if (queue.Count > 0)
                {
                    try
                    {
                        IndexHandler();  //开始将队列中的内容插入索引库
                    }
                    catch (Exception ex)  //接收异常，并记录日志。不要给用户弹对话框
                    {
                        Console.WriteLine(ex.ToString()); 
                        //换为Log4Net记录日志
                    }
                }
                else
                {
                    Thread.Sleep(10000); 
                }
            }
        }

        /// <summary>
        /// Lucene 倒排索引处理
        /// 开始→数据→文本→分词器→Field对象→Document对象→IndexWriter→Directory→结束
        /// </summary>
        private void IndexHandler()
        {
            //将创建的分词内容放在该目录下.
            string indexPath = System.Configuration.ConfigurationManager.AppSettings["IndexPath"];

            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NativeFSLockFactory());

            bool isUpdate = IndexReader.IndexExists(directory);
            if (isUpdate)
            {
                if (IndexWriter.IsLocked(directory))
                {
                    IndexWriter.Unlock(directory);
                }
            }

            IndexWriter writer = new IndexWriter(directory, new PanGuAnalyzer(), !isUpdate, IndexWriter.MaxFieldLength.UNLIMITED);

            //循环遍历队列
            while (queue.Count > 0)
            {
                IndexTask task = queue.Dequeue();
                writer.DeleteDocuments(new Term("id", task.Id.ToString()));

                if (task.Type == TaskType.Delete)
                {
                    continue;
                }

                Document document = new Document();

                document.Add(new Field("id", task.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

                document.Add(new Field("title", task.Title, Field.Store.YES, Field.Index.ANALYZED, Lucene.Net.Documents.Field.TermVector.WITH_POSITIONS_OFFSETS));

                document.Add(new Field("content", task.Content, Field.Store.YES, Field.Index.ANALYZED, Lucene.Net.Documents.Field.TermVector.WITH_POSITIONS_OFFSETS));

                writer.AddDocument(document);
            }

            writer.Close();
            directory.Close();
        }
    }
}
