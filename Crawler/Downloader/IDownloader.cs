
using Crawler.Core.Scheduler;

namespace Crawler.Core.Downloader
{
    /// <summary>
    /// 下载器
    /// </summary>
    public interface IDownloader
    {
        /// <summary>
        /// 在一个网页下载或JS渲染开始之前调用, 主要用来处理网页url
        /// </summary>
        /// <param name="page"></param>
        /// <param name="schduler"></param>
        /// <returns>处理后的网页</returns>
        Page BeforeDownloadPage(Page page, ISchduler schduler);

        /// <summary>
        /// 在切换代理IP后调用, 主要用来给HTTP请求添加Header和Cookie等数据
        /// </summary>
        /// <param name="schduler"></param>
        /// <param name="page"></param>
        void OnChangeProxy(ISchduler schduler, Page page );
        /// <summary>
        /// 判断访问网页时是否被目标网站屏蔽,如果判断被目标网站屏蔽, 返回true; 如果判断未被目标网站屏蔽, 返回false, 如果判断被屏蔽了, 会切换一次代理IP后自动重新爬取(前提: 开启代理IP切换)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <param name="page"></param>
        bool IsAntiSpider(string url , string content , Page page );

        /// <summary>
        /// 在一个网页下载或JS渲染完成之后调用, 主要用来处理网页
        /// </summary>
        /// <param name="page"></param>
        /// <param name="schduler"></param>
        /// <returns>处理后的网页</returns>
        Page AfterDownloadPage(Page page , ISchduler schduler);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        void SetCrawler(Crawler c);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="schduler"></param>
        /// <returns></returns>
        Page Download(Request r, ISchduler schduler);
    }
}