using System;
using System.Collections.Generic;

namespace Crawler.Core.Scheduler
{
    public interface ISchduler
    {
        /// <summary>
        /// 向调试中心增加一个地址
        /// </summary>
        /// <param name="url"></param>
        void AddUrl(string url);
        /// <summary>
        /// 向调试中心增加很多个地址
        /// </summary>
        /// <param name="urls"></param>
        void AddUrls(IEnumerable<string> urls);


        Request GetNext();


        void AddScanUrl(string url);
        int Left { get; }
      
    }
}