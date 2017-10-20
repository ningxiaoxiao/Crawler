using System.Collections.Generic;
using Crawler.Core.Downloader;
using Crawler.Core.Processor;

namespace Crawler.Core
{
    /// <summary>
    /// 表示当前正在爬取的网页对象
    /// </summary>
    public class Page : HttpResult
    {
        public Page() { }

        public Page(HttpResult r)
        {
            Html = r.Html;
        }

        /// <summary>
        /// 当前网页的内容. 通过自动JS渲染的网页, page.raw是JS渲染后的网页内容
        /// </summary>
        /// <summary>
        /// 请求page网页时使用的请求参数等数据
        /// </summary>
        public Request Request { get; set; }
        /// <summary>
        /// 请求page网页返回的响应信息
        /// </summary>
        public System.Net.HttpWebResponse Response { get; set; }
        /// <summary>
        /// 当前网页的附加数据, 是开发者自定义的一段代码, 例如, HTML代码.
        /// </summary>
        public string ContextData { get; set; }


        public List<Result> Results { get; } = new List<Result>();

        /// <summary>
        /// 跳过抽取
        /// </summary>
        public bool SkipExtractField { get; set; } = false;

        /// <summary>
        /// 跳过从这个网页正文发现新的URL
        /// </summary>
        public bool SkipFindUrl { get; set; } = false;

        /// <summary>
        /// 只能在afterExtractField回调函数中使用, 
        /// 用来过滤抽取项中不需要的抽取结果, 
        /// 结果不会被保存到数据库中
        /// </summary>
        public bool SkipSave { get; set; } = false;
    }
}