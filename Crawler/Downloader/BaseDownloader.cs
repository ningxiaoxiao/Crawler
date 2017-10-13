using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler.Core.Scheduler;
using DotNet.Utilities;

namespace Crawler.Core.Downloader
{

    /// <summary>
    /// 从调试中心拿到地址,下载后送到处理中心
    /// </summary>
    public class BaseDownloader : IDownloader
    {
        
        readonly HttpHelper _http = new HttpHelper();
        public Crawler Crawler { get; private set; }


        public Page AfterDownloadPage(Page page, ISchduler site)
        {
            throw new NotImplementedException();
        }

        public void SetCrawler(Crawler c)
        {
            Crawler = c;
        }

        public Page Download(Request r,ISchduler schduler)
        {
            var p = _http.GetHtml(r) as Page;
            p.Request = r;
            //把p的人cookie存到总cookie中去
            schduler.AddCookie(p.CookieCollection);
            return p;
        }

        public Page BeforeDownloadPage(Page page, ISchduler site)
        {
            throw new NotImplementedException();
        }



        public bool IsAntiSpider(string url, string content, Page page)
        {
            throw new NotImplementedException();
        }

        public void OnChangeProxy(ISchduler site, Page page)
        {
            throw new NotImplementedException();
        }
    }
}
