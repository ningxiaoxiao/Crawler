using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Crawler.Core.Scheduler.Tests
{
    [TestClass()]
    public class BaseSchdulerTests:BaseSchduler
    {
        [TestMethod()]
        public void GetNextTest()
        {
           Queue<string> a=new Queue<string>();

            a.
            AddUrl("http://www.baidu.com");

            Assert.AreEqual(Queue);
        }
    }

}