using Crawler.Core.Downloader;
using Crawler.Core.Pipeline;
using Crawler.Core.Processor;
using Crawler.Core.Scheduler;

namespace Crawler.Core
{
   public class Crawler
    {
        public IDownloader Downloader { get; private set; }
        public  IPipeline Pipeline { get; private set; }
        public  IProcessor Processor { get; private set; }
        public  ISchduler Schduler { get; private set; }

        public delegate void VoidDelegate();


        public string Identity { get; private set; }

        public Crawler(Config c)
        {
            
        }

        public Crawler(string url,string identity, IDownloader downloader,IPipeline pipeline,IProcessor processor,ISchduler schduler)
        {
            Identity = identity;
            Downloader = downloader;
            Pipeline = pipeline;
            Processor = processor;
            Schduler = schduler;
            Schduler.AddScanUrl(url);

        }

        public void Run()
        {
            BeforeCrawl?.Invoke();

            while (Schduler.Left>0)
            {
                var r = Schduler.GetNext();
                var p= Downloader.Download(r);
                Processor.Process(p);
                Processor.SaveResults += Pipeline.Process;
            }
        }

        ///<summary>
        /// 初始化时调用, 用来进行一些爬取前的操作, 栗如, 给所有HTTP请求添加Headers等
        /// </summary>
        private event VoidDelegate BeforeCrawl;
        

        public void beforeDownloadPage()
        {
            
        }

        protected virtual void OnBeforeCrawl()
        {
            BeforeCrawl?.Invoke();
        }
    }
}
