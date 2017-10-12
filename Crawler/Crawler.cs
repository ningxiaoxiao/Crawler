using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Crawler.Core.Downloader;
using Crawler.Core.Pipeline;
using Crawler.Core.Processor;
using Crawler.Core.Scheduler;

namespace Crawler.Core
{
   public class Crawler
    {
        IDownloader _downloader;
        IPipeline _pipeline;
        IProcessor _processor;
        ISchduler _schduler;


        string _identity;
        Crawler(string identity, IDownloader downloader,IPipeline pipeline,IProcessor processor,ISchduler schduler)
        {

        }

    }
}
