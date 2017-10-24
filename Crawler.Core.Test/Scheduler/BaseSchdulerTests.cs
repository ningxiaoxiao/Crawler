using Crawler.Core.Scheduler;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Crawler.Core.Scheduler.Tests
{

    [TestClass()]
    public class BaseSchdulerTests : BaseSchduler
    {
        [TestMethod()]
        public void GetNextTest()
        {

        }

        [TestMethod()]
        public void AddCookieTest()
        {
            var c = new Crawler();
            c.Setup(new Config { ScanUrls = "http://www.baidu.com" });
            c.Schduler.AddCookie("hi", "nihao");

            var r = c.Schduler.GetCookie("hi", "www.baidu.com");

            Assert.AreEqual("nihao", r);
        }
    }

}