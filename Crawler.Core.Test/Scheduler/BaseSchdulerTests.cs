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

            var s=new BaseSchdulerTests();
            var c=new Crawler();
            c.Setup(new Config(){Domains = new []{"www.baidu.com"}});
            s.AddCookie("hi","nihao");

            var r = s.GetCookie("hi","www.baidu.com");

            Assert.AreEqual(r,"nihao");
        }
    }

}