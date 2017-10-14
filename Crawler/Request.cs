using Crawler.Core.Downloader;
using Crawler.Core.Scheduler;

namespace Crawler.Core
{
    /// <summary>
    /// 请求的实体
    /// </summary>
    public class Request:HttpItem
    {
        public Request(ISchduler schduler)
        {
            Schduler = schduler;
        }
        /// <summary>
        /// 调度器
        /// </summary>
        public ISchduler Schduler { get; }
        /// <summary>
        /// 尝试几次,0-1都会只尝试一次,
        /// </summary>
        public int TryTimes { get; set; } = 0;
    }
}