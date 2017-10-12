using System;

namespace Crawler.Core.Scheduler
{
    public interface ISchduler
    {
        /// <summary>
        /// 处理结果
        /// </summary>
        /// <param name="resultItems">抽取出来的结果</param>
        void Process(IDisposable resultItems);
    }
}