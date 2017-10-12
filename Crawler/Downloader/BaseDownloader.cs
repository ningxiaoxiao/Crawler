using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


        public Page AfterDownloadPage(Page page, Site site)
        {
            throw new NotImplementedException();
        }

        public void SetCrawler(Crawler c)
        {
            Crawler = c;
        }

        public Page BeforeDownloadPage(Page page, Site site)
        {
            throw new NotImplementedException();
        }

        public Page Download(Site site)
        {
            var hi = new HttpItem()
            {
                URL = site.Url
            };

            var res = _http.GetHtml(hi);

            var p = new Page(res.Html) ;
            return p;
        }

        public bool IsAntiSpider(string url, string content, Page page)
        {
            throw new NotImplementedException();
        }

        public void OnChangeProxy(Site site, Page page)
        {
            throw new NotImplementedException();
        }
    }
}
