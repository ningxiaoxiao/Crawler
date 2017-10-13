using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Crawler.Core.Downloader;

namespace Crawler.Core.Test
{
    [TestClass]
    public class DownloaderTest
    {
        [TestMethod]
        public void Download()
        {
            var k = new BaseDownloader();
            var s = new Scheduler.Tests.BaseSchdulerTests();
            var c = new Crawler();
            c.Setup(new Config() { Domains = new[] { "www.baidu.com" } });
            s.Bind(c);


            s.AddUrl("https://www.baidu.com/img/baidu.svg");
            var p = k.Download(s.GetNext(),s);
            
            Console.WriteLine(s.GetCookie("BAIDUID", "www.baidu.com"));
        }
    }
}
