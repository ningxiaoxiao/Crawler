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
    public sealed class Crawler
    {
        public static Logger Logger = LogManager.GetLogger("Crawler");
        private static object _lock = new object();
        public IDownloader Downloader { get; private set; }
        public IPipeline Pipeline { get; private set; }
        public IProcessor Processor { get; private set; }
        public ISchduler Schduler { get; private set; }


        public static Config Config { get; private set; }



        public delegate void VoidDelegate();


        public void Setup(Config c)
        {

            if (c.ScanUrls == null) throw new Exception("没有启动页");
            Config = c;

            Downloader = new BaseDownloader();
            Schduler = new DefaultSchduler();
            Processor = new DefaultProcessor();
            Pipeline = new MySqlPipline();

            Downloader.DownloadComplete = Processor.Handle;
            Processor.OnComplete = Pipeline.Handle;

            Schduler.AddScanUrl(Config.ScanUrls);

            ThreadPool.SetMaxThreads(Config.ThreadNum * 5, Config.ThreadNum * 5);
        }

        public void Run()
        {
            Logger.Info("启动");
            BeforeCrawl?.Invoke();
            var sw = new Stopwatch();
            sw.Start();
            Logger.Info("启动10个线程");

            for (var i = 0; i < Config.ThreadNum; i++)
            {

                ThreadPool.QueueUserWorkItem(Task);

            }

            sw.Stop();

            Logger.Info($"启动完成 耗时:{sw.ElapsedMilliseconds}ms");
        }

        private void Task(object state)
        {
            Logger.Info($"[{Thread.CurrentThread.ManagedThreadId}]号 线程启动");
            var sleep = false;
            var sleeptime = Config.Interval;

            while (true)
            {
                var sw = new Stopwatch();

                sw.Start();
                Request r;
                int waittime = 0;
                do
                {
                    r = Schduler.GetNext();
                    if (r == null)
                    {
                        Thread.Sleep(1000);
                        waittime += 1000;
                        if (waittime >= 30000)
                        {
                            Logger.Info($"已经连续30秒没有得到新的请求,[{Thread.CurrentThread.ManagedThreadId}]号线程进入睡眠");
                            sleep = true;
                            break;
                        }
                    }
                    else
                    {
                        waittime = 0;
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
                if (!sleep) continue;
                Logger.Info("睡眠中.睡眠时间:" + sleeptime);
                while (true)
                {
                    
                    Thread.Sleep(5000);
                    sleeptime -= 5;
                    if (sleeptime <= 0)
                        break;

                }
                Logger.Info("睡眠完成");
                sleep = false;
                sleeptime = Config.Interval;
            }


        }
        protected void OnBeforeCrawl()
        {
            BeforeCrawl?.Invoke();
        }
        ///<summary>
        /// 初始化时调用, 用来进行一些爬取前的操作, 栗如, 给所有HTTP请求添加Headers等
        /// </summary>
        private event VoidDelegate BeforeCrawl;


    }
}
