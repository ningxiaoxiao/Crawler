﻿using System;
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
        private static readonly object _lock = new object();

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
                //下载失败
                FailCount++;
                var failres = p.Response == null ? p.Html : p.Response.StatusDescription;
                Logger.Warn($"下载 {p.Request.URL} 失败,{failres}");
                if (r.LeftTryTimes > 0) p.Request.Schduler.AddRequest(beforR);
                return;
            }
            SuccessCount++;
            Logger.Info($"下载 {p.Request.URL} 成功");

            //把p的cookie存到总cookie中去
            if (p.CookieCollection.Count > 0)
                beforR.Schduler.AddCookie(p.CookieCollection);
            DownloadComplete?.Invoke(p);
            AfterDownloadPage?.Invoke(p);
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
