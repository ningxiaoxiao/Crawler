using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Crawler.Core.Downloader;

namespace Crawler.Core.Test
{
    [TestClass]
    public class DownloaderTest
    {
        [TestMethod]
        public void download()
        {
            var k = new BaseDownloader();
            var s = new Site();
            s.AddUrl("https://www.douyu.com/directory/all");
            var p = k.Download(s);
            Console.WriteLine(p.Raw);
        }
    }
}
