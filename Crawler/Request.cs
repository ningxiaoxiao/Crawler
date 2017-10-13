using DotNet.Utilities;

namespace Crawler.Core
{
    /// <summary>
    /// 请求的实体
    /// </summary>
    public class Request:HttpItem
    {
        public int TryTimes { get; set; } = 0;
    }
}