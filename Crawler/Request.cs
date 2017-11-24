using System;
using CrawlerDotNet.Core.Downloader;
using CrawlerDotNet.Core.Scheduler;

namespace CrawlerDotNet.Core
{
    /// <summary>
    /// 请求的实体
    /// </summary>
    public class Request : HttpItem
    {
        public PageType Type { get; private set; }
        //todo 去掉从这里使用调度器
        public Request(ISchduler schduler)
        {
            Schduler = schduler;
        }

        public void SetType(PageType t)
        {
            Type = t;
        }
        /// <summary>
        /// 调度器
        /// </summary>
        public ISchduler Schduler { get; }

        public int Deth { get; set; } = 0;
        /// <summary>
        /// 剩余尝试下载次数
        /// </summary>
        public int LeftTryTimes { get; set; } 
        public static bool operator ==(Request a, Request b)
        {
            if (Equals(a, null) && Equals(b, null))
                return true;

            if (!Equals(a, null) && !Equals(b, null))
                return a.Url == b.Url;
            return false;
        }

        public static bool operator !=(Request a, Request b)
        {
            if (Equals(a, null) && Equals(b, null))
                return false;

            if (!Equals(a, null) && !Equals(b, null))
                return a.Url != b.Url;
            return false;

        }

        public override bool Equals(object obj)
        {
            if (Equals(obj, null)) return false;
            var r = (Request)obj;

            return Url == r.Url;
        }

        public override int GetHashCode()
        {
            return Url.GetHashCode();
        }
    }


}