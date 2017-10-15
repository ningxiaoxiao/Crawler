using System;
using System.Net;
using Crawler.Core.Processor;
using Crawler.Core.Scheduler;
using NLog;

namespace Crawler.Core.Downloader
{
    public delegate Request RequestDelegate(Request r);

    /// <summary>
    /// 从调试中心拿到地址,下载后送到处理中心
    /// </summary>
    public class BaseDownloader : IDownloader
    {

        public VoidPageDelegate AfterDownloadPage { get; set; }
        public RequestDelegate BeforeDownloadPage { get; set; }
        public VoidPageDelegate DownloadComplete { get; set; }
        public int SuccessCount { get; private set; }
        public int FailCount { get; private set; }


        public Logger Logger => Crawler.Logger;
        private readonly HttpHelper _http = new HttpHelper();
        private static object _lock=new object();

        public void Download(Request r)
        {
            lock (_lock)
            {
                if(r==null)return;
                var beforR = BeforeDownloadPage?.Invoke(r) ?? r;
                var p = _http.GetHtml(beforR) as Page;
                p.Request = beforR;
                if (p.Response.StatusCode != HttpStatusCode.OK)
                {
                    //下载失败
                    FailCount++;
                    Logger.Warn($"下载 {p.Request.URL} 失败,{p.Response.StatusCode}");
                    return;
                }
                SuccessCount++;
                Logger.Info($"下载 {p.Request.URL} 成功");

                //把p的cookie存到总cookie中去
                if (p.CookieCollection != null)
                    beforR.Schduler.AddCookie(p.CookieCollection);
                DownloadComplete?.Invoke(p);
                AfterDownloadPage?.Invoke(p);
            }
           

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
