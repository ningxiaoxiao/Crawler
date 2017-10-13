using System;
using System.Collections.Generic;

namespace Crawler.Core.Processor
{
    public delegate void ResultDelegate(IEnumerable<KeyValuePair<string, dynamic>> results);
    public interface IProcessor
    {
        
        void Process(Page page);
        event ResultDelegate SaveResults;
        void Config(Config config);
    }
}