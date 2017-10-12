using System;
using System.Collections.Generic;

namespace Crawler.Core.Scheduler
{
    public abstract class BaseSchduler:ISchduler
    {
        private readonly Queue<Request> _queue=new Queue<Request>();
        
        public void AddUrl(string url)
        {
            _queue.Enqueue(new Request {URL = url});
        }

        public void AddUrls(IEnumerable<string> urls)
        {

            foreach (var u in urls)
            {
                AddUrl(u);
            }
            
        }

        public Request GetNext()
        {
            return _queue.Dequeue();
        }

  

        public void AddScanUrl(string url)
        {
            throw new NotImplementedException();
        }

        public int Left => _queue.Count;
    }
}