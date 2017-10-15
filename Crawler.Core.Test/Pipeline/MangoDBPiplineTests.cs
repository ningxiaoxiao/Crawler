using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crawler.Core.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crawler.Core.Processor;

namespace Crawler.Core.Pipeline.Tests
{
    [TestClass()]
    public class MangoDBPiplineTests
    {
        [TestMethod()]
        public void HandleTest()
        {

            var p=new MangoDBPipline();
            var page=new Page();
            page.Results.Add(new Result("name","ningxiaoxiao"));
            p.Handle(page);


        }
    }
}