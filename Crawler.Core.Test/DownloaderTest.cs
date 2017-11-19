using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CrawlerDotNet.Core.Downloader;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace CrawlerDotNet.Core.Test
{
    [TestClass]
    public class DownloaderTest
    {
        [TestMethod]
        public void Download()
        {

            var c = new Crawler();
            c.Setup(new Config {ScanUrls = "http://www.baidu.com" });
            c.Schduler.AddScanUrl("https://www.baidu.com");

            c.Downloader.Download(c.Schduler.GetNext());

            Assert.AreEqual("zh", c.Schduler.GetCookie("local", "baidu.com"));

        }
    }
}
