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

        public void SetType(PageType t)
        {
            Type = t;
        }


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