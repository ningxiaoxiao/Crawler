using System;

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
        void AddUrls(IDisposable urls);

        void GetNextUrl()
        {
            
        }

        /// <summary>
        /// 处理结果
        /// </summary>
        /// <param name="resultItems">抽取出来的结果</param>
        void Process(IDisposable resultItems);
    }
}