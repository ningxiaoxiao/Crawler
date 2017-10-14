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
        public IDownloader Downloader { get; private set; }
        public IPipeline Pipeline { get; private set; }
        public IProcessor Processor { get; private set; }
        public ISchduler Schduler { get; private set; }

        public delegate void VoidDelegate();


        public string Identity { get; private set; }

        public Config Config { get; private set; }



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

            Schduler.AddScanUrl(c.ScanUrls);
        }

        public void Run()
        {
            BeforeCrawl?.Invoke();
            var sw = new Stopwatch();
            sw.Start();
            while (Schduler.Left > 0)
            {
                var st = sw.ElapsedMilliseconds;
                var r = Schduler.GetNext();
                Downloader.Download(r);

                Logger.Info($"剩余:{Schduler.Left}  下载失败:{Downloader.FailCount} 解析成功:{Processor.ExtractCount} " +
                            $"解析失败:{Processor.FailCount} 跳过解析:{Processor.SkipCount} " +
                            $"总计:{Downloader.SuccessCount + Downloader.FailCount} " +
                            $"上一轮用时:{sw.ElapsedMilliseconds - st}ms 总用时:{sw.Elapsed.TotalSeconds}s");


            }
            sw.Stop();

            var sleeptime = (int)((double)Config.Interval * 1000 - sw.ElapsedMilliseconds);
            if (sleeptime > 0 && Schduler.Left > 0)
            {
                Console.WriteLine("Crawler.Run sleep:" + sleeptime);
                Thread.Sleep(sleeptime);
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
