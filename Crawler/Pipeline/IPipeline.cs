using System;
using System.Collections.Generic;

namespace Crawler.Core.Pipeline
{
    public interface IPipeline
    {
        void Process(IEnumerable<KeyValuePair<string, dynamic>> results);
    }
}