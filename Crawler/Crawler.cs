using System;
using System.Diagnostics;
using System.Threading;
using Crawler.Core.Downloader;
using Crawler.Core.Pipeline;
using Crawler.Core.Processor;
using Crawler.Core.Scheduler;
using NLog;

namespace Crawler.Core
{
    public class Crawler
    {
        public static Logger Logger = LogManager.GetLogger("Crawler");
        private static object _lock = new object();
        public IDownloader Downloader { get; private set; }
        public IPipeline Pipeline { get; private set; }
        public IProcessor Processor { get; private set; }
        public ISchduler Schduler { get; private set; }

        public string Identity { get; private set; }

        public Config Config { get; private set; }



        public delegate void VoidDelegate();


        public void Setup(Config c)
        {
            Config = c;

            Downloader = new BaseDownloader();
            Schduler = new DefaultSchduler();
            Processor = new DefaultProcessor();
            Pipeline = new MangoDBPipline();

            Schduler.Config = c;
            Processor.Config = c;
            Downloader.DownloadComplete = Processor.Handle;
            Processor.OnComplete = Pipeline.Handle;

            if (c.ScanUrls == null) throw new Exception("没有启动页");
            Schduler.AddScanUrl(c.ScanUrls);
            ThreadPool.SetMaxThreads(c.ThreadNum, c.ThreadNum);
        }

        public void Run()
        {
            BeforeCrawl?.Invoke();
            var sw = new Stopwatch();
            sw.Start();
            var retrytimes = 0;

            for (int i = 0; i < Config.ThreadNum; i++)
            {
                ThreadPool.QueueUserWorkItem(Task);
            }

            sw.Stop();

            Logger.Info($"启动完成 耗时:{sw.ElapsedMilliseconds}ms");
        }


        public void beforeDownloadPage()
        {

        }

        private void Task(object state)
        {
            int sleeptime = 0;
            while (true)
            {
                var sw = new Stopwatch();
                sw.Start();
                Request r;
                do
                {
                    r = Schduler.GetNext();
                    if (r == null)
                    {
                        Thread.Sleep(1000);
                        sleeptime += 1000;
                        if (sleeptime > 30000)
                        {
                            Logger.Warn("已经30秒没有得到新的请求,线程退出");
                            return;
                        }
                    }
                } while (r == null);



                Downloader.Download(r);
                sw.Stop();
                var tc = 0;
                var twc = 0;
                ThreadPool.GetAvailableThreads(out tc, out twc);
                Logger.Info($"剩余:{Schduler.Left}  下载失败:{Downloader.FailCount} 解析成功:{Processor.ExtractCount} " +
                            $"解析失败:{Processor.FailCount} 跳过解析:{Processor.SkipCount} " +
                            $"总计:{Downloader.SuccessCount + Downloader.FailCount} " +
                            $"用时:{sw.ElapsedMilliseconds}ms 激活线程数:{twc - tc}");
            }

        }
        protected virtual void OnBeforeCrawl()
        {
            BeforeCrawl?.Invoke();
        }
        ///<summary>
        /// 初始化时调用, 用来进行一些爬取前的操作, 栗如, 给所有HTTP请求添加Headers等
        /// </summary>
        private event VoidDelegate BeforeCrawl;


    }
}
