
using Crawler.Core.Processor;
using Crawler.Core.Scheduler;
using NLog;

namespace Crawler.Core.Downloader
{
    /// <summary>
    /// 下载器
    /// </summary>
    public interface IDownloader
    {
        int SuccessCount { get; }
        int FailCount { get; }
        Logger Logger { get; }
        /// <summary>
        /// 在一个网页下载开始之前调用, 主要用来处理网页url
        /// </summary>
        /// <returns>处理后的</returns>
        RequestDelegate BeforeDownloadPage { get; set; }

        /// <summary>
        /// 在一个网页下载或JS渲染完成之后调用, 主要用来处理网页
        /// </summary>
        /// <param name="page"></param>
        /// <returns>处理后的网页</returns>
        VoidPageDelegate AfterDownloadPage { get; set; }
        VoidPageDelegate DownloadComplete { get; set; }
        /// <summary>
        /// 在切换代理IP后调用, 主要用来给HTTP请求添加Header和Cookie等数据
        /// </summary>
        /// <param name="schduler"></param>
        /// <param name="page"></param>
        void OnChangeProxy(ISchduler schduler, Page page);
        /// <summary>
        /// 判断访问网页时是否被目标网站屏蔽,如果判断被目标网站屏蔽, 返回true; 如果判断未被目标网站屏蔽, 返回false, 如果判断被屏蔽了, 会切换一次代理IP后自动重新爬取(前提: 开启代理IP切换)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <param name="page"></param>
        bool IsAntiSpider(string url, string content, Page page);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        void Download(Request r);
    }
}