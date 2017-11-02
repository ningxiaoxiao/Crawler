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
        /// <summary>
        /// 全局配置
        /// </summary>
        public static Config Config { get; private set; }

        /// <summary>
        /// 全局logger
        /// </summary>
        public static Logger Logger { get; private set; }
        /// <summary>
        /// 本次抓取唯一标识
        /// </summary>
        public static DateTime CurStartTime { get; private set; }

        private static object _lock = new object();
        private int _runTimes = 0;
        private DateTime _nextRunTime;
        private Timer _timer;

        public BaseDownloader Downloader { get; }
        public IPipeline Pipeline { get; private set; }
        public DefaultProcessor Processor { get; }
        public DefaultSchduler Schduler { get; }


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
            _nextRunTime = Config.RepeatAt;
            //todo 从配置文件得到服务器配置
            Pipeline = new MySqlPipline(Config.MysqlConString);

            Downloader.DownloadComplete = Processor.Handle;
            Processor.OnComplete = Pipeline.Handle;
            Logger.Info("配置完成,等待运行时间:" + Config.RepeatAt);

        }

        public void Start()
        {
            _timer = new Timer(OnTimer, null, 1000, 1000);
        }

        private void OnTimer(object o)
        {
            if (DateTime.Now < _nextRunTime) return;
            Logger.Info($"到了运行时间{_nextRunTime},启动主线程中...");
            CurStartTime = DateTime.Now;
            Schduler.Reset();
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
            Schduler.AddScanUrl(Config.ScanUrls);
            
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
                    _nextRunTime = _nextRunTime.AddHours(1);
                    break;
                case RepeatWhenEver.day:
                    _nextRunTime = _nextRunTime.AddDays(1);
                    break;
                case RepeatWhenEver.week:
                    _nextRunTime = _nextRunTime.AddDays(7);
                    break;
                case RepeatWhenEver.month:
                    _nextRunTime = _nextRunTime.AddMonths(1);
                    break;
                case RepeatWhenEver.min:
                    _nextRunTime = _nextRunTime.AddMinutes(1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Logger.Info($"下次运行时间:{_nextRunTime}");
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
