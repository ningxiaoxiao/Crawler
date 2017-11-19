using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrawlerDotNet.Core.Processor;

namespace CrawlerDotNet.Core.Pipeline.Tests
{
    [TestClass()]
    public class MangoPiplineTests
    {
        [TestMethod()]
        public void HandleTest()
        {

            var p = new MangoPipline();
            var page = new Page();
            page.Results.Add(
                new ExtractResults
                {
                    new Result("name", "ningxiaoxiao")
                });
            p.Handle(page);
        }
    }
}