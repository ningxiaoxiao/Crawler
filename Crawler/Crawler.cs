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
        public static Logger Logger { get; private set; }
        private static object _lock = new object();
        private int _runTimes = 0;
        private DateTime _nexTime;
        private Timer _timer;

        public BaseDownloader Downloader { get; }
        public IPipeline Pipeline { get; private set; }
        public DefaultProcessor Processor { get; }
        public DefaultSchduler Schduler { get; }


        public static Config Config { get; private set; }

        ~Crawler()
        {
            Logger.Info("对象销毁");
        }

        public Crawler()
        {
            Downloader = new BaseDownloader();
            Schduler = new DefaultSchduler();
            Processor = new DefaultProcessor();


        }
        public void Setup(Config c)
        {

            if (c.ScanUrls == null) throw new Exception("没有启动页");
            Config = c;
            Logger = LogManager.GetLogger(Config.Name);
            Logger.Info(c.ToString());
            _nexTime = Config.RepeatAt;

            Pipeline = new MySqlPipline();

            Downloader.DownloadComplete = Processor.Handle;
            Processor.OnComplete = Pipeline.Handle;
            Logger.Info("配置完成,等待运行时间:" + Config.RepeatAt);
            Schduler.AddScanUrl(Config.ScanUrls);
        }

        public void Start()
        {
            _timer = new Timer(OnTimer, null, 1000, 1000);
        }

        private void OnTimer(object o)
        {
            if (DateTime.Now < _nexTime) return;
            Logger.Info($"到了运行时间{_nexTime},启动主线程中...");
            new Thread(Run).Start();
            _runTimes++;

            CallNextRun();
        }

        private void Run()
        {
            Logger.Info("进入主线程");
            var sw = new Stopwatch();
            sw.Start();
            BeforeCrawl?.Invoke();


            Logger.Info($"启动{Config.ThreadNum}个线程");
            for (var i = 0; i < Config.ThreadNum; i++)
            {

                new Thread(Task).Start();

            }

            sw.Stop();

            Logger.Info($"启动处理线程完成 耗时:{sw.ElapsedMilliseconds}ms");



        }

        private void CallNextRun()
        {
            Logger.Info("设置下次运行时间");
            switch (Config.RepeatWhen)
            {
                case RepeatWhenEver.once:
                    if (_runTimes > 0)
                    {
                        Logger.Info("运行一次后退出");
                        _timer.Dispose();

                    }
                    return;
                case RepeatWhenEver.hour:
                    _nexTime = _nexTime.AddHours(1);
                    break;
                case RepeatWhenEver.day:
                    _nexTime = _nexTime.AddDays(1);
                    break;
                case RepeatWhenEver.week:
                    _nexTime = _nexTime.AddDays(7);
                    break;
                case RepeatWhenEver.month:
                    _nexTime = _nexTime.AddMonths(1);
                    break;
                case RepeatWhenEver.min:
                    _nexTime = _nexTime.AddMinutes(1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Logger.Info($"下次运行时间:{_nexTime}");
        }


        private void Task(object state)
        {
            Logger.Info($"[{Thread.CurrentThread.ManagedThreadId}]号 线程启动");
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
                            Logger.Info($"已经连续30秒没有得到新的请求,[{Thread.CurrentThread.ManagedThreadId}]号线程停止");
                            return;
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
                Logger.Info($"剩余:{Schduler.Left}  下载失败:{Downloader.FailCount} 解析成功:{Processor.ExtractCount} " + $"解析失败:{Processor.FailCount} 跳过解析:{Processor.SkipCount} " + $"总计:{Downloader.SuccessCount + Downloader.FailCount} " + $"用时:{sw.ElapsedMilliseconds}ms 激活线程数:{twc - tc}");
            }
        }

        public delegate void VoidDelegate();


        ///<summary>
        /// 初始化时调用, 用来进行一些爬取前的操作, 栗如, 给所有HTTP请求添加Headers等
        /// </summary>
        private event VoidDelegate BeforeCrawl;
    }
}
