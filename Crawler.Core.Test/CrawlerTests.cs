using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crawler.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crawler.Core.Downloader;

namespace Crawler.Core.Tests
{
    [TestClass()]
    public class CrawlerTests
    {
        [TestMethod()]
        public void RunTest()
        {
            var config = new Config {Fields = new[] {new Field(){Name="aaa"},}};
            var c=new Crawler("http://www.douyu.com","a",new BaseDownloader(), );


            Assert.Fail();
        }
    }
}

