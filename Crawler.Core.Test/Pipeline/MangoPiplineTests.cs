using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crawler.Core.Processor;

namespace Crawler.Core.Pipeline.Tests
{
    [TestClass()]
    public class MangoPiplineTests
    {
        [TestMethod()]
        public void HandleTest()
        {

            var p=new MangoPipline();
            var page=new Page();
            page.Results.Add(new Result("name","ningxiaoxiao"));
            p.Handle(page);
        }
    }
}