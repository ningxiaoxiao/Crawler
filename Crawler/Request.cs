using System;
using Crawler.Core.Downloader;
using Crawler.Core.Scheduler;

namespace Crawler.Core
{
    /// <summary>
    /// 请求的实体
    /// </summary>
    public class Request : HttpItem
    {
        public Request(ISchduler schduler)
        {
            Schduler = schduler;
        }
        /// <summary>
        /// 调度器
        /// </summary>
        public ISchduler Schduler { get; }

        public int Deth { get; set; }= 0;
        /// <summary>
        /// 尝试几次,0-1都会只尝试一次,
        /// </summary>
        public int TryTimes { get; set; } = 0;
        public static bool operator ==(Request a, Request b)
        {
            if (Equals(a, null) && Equals(b, null))
                return true;

            if (!Equals(a, null) && !Equals(b, null))
                return a.URL == b.URL;
            return false;
        }

        public static bool operator !=(Request a, Request b)
        {
            if (Equals(a, null) && Equals(b, null))
                return false;

            if (!Equals(a, null) && !Equals(b, null))
                return a.URL != b.URL;
            return false;

        }

        public override bool Equals(object obj)
        {
            if (Equals(obj, null)) return false;
            var r = (Request)obj;

            return URL == r.URL;
        }

        public override int GetHashCode()
        {
            return URL.GetHashCode();
        }
    }
}