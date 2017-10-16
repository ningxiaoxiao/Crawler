using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Crawler.Core.Downloader;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Crawler.Core.Test
{
    [TestClass]
    public class DownloaderTest
    {
        [TestMethod]
        public void Download()
        {

            var c = new Crawler();
            c.Setup(new Config() { ScanUrls = "https://www.baidu.com/img/baidu.svg" });


            c.Downloader.Download(c.Schduler.GetNext());

            Logger.LogMessage(c.Schduler.GetCookie("BAIDUID", "www.baidu.com"));
        }
    }
}
