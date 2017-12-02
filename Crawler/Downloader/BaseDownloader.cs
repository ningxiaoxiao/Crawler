using System;
using System.Net;
using CrawlerDotNet.Core.Processor;
using CrawlerDotNet.Core.Scheduler;
using NLog;

namespace CrawlerDotNet.Core.Downloader
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
        private static readonly object _lock = new object();

        public Page DownloaderOnly(Request r)
        {
            if (r == null) return null;
            var http = new HttpHelper();
            var p = (Page)http.GetHtml(r);
            p.Request = r;
            return p;
        }
        public void Download(Request r)
        {
            if (r == null) return;
            r.LeftTryTimes--;

            var beforR = BeforeDownloadPage?.Invoke(r) ?? r;

            var http = new HttpHelper();
            var p = (Page)http.GetHtml(beforR);
            p.Request = beforR;

            if (p.Response == null || p.Response.StatusCode != HttpStatusCode.OK)
            {

                var failres = p.Response == null ? p.Html : p.Response.StatusDescription;

                if (p.Response != null && p.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    //找不到的页面.去掉
                    return;
                }

                if (r.LeftTryTimes > 0)
                {
                    Logger.Warn($"下载 {p.Request.Url} 失败,原因:{failres},剩余重试次数:{r.LeftTryTimes}");

                    Crawler.inst.Schduler.AddRequest(r);
                }
                else
                {
                    //下载失败
                    FailCount++;
                    Logger.Warn($"下载 {p.Request.Url} 失败,重试次数用完,当前配置重试次数:{Crawler.Config.TryTimes}");
                }


                return;
            }
            SuccessCount++;
            Logger.Info($"下载 {p.Request.Url} 成功");

            //把p的cookie存到总cookie中去
            if (p.CookieCollection.Count > 0)
                Crawler.inst.Schduler.AddCookie(p.CookieCollection);
            AfterDownloadPage?.Invoke(p);
            DownloadComplete?.Invoke(p);

        }

        private static string GetTimestamp(bool l = false)
        {
            return ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / (l ? 10000 : 10000000)).ToString();
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
