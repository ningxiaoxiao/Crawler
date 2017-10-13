using System;
using System.Collections.Generic;
using System.Net;

namespace Crawler.Core.Scheduler
{
    public abstract class BaseSchduler : ISchduler
    {


        private readonly Queue<Request> _queue = new Queue<Request>();

        public Crawler Crawler { get; private set; }


        public Request GetNext()
        {
            return _queue.Dequeue();
        }

        public void AddScanUrl(string url)
        {
            throw new NotImplementedException();
        }

        public int Left => _queue.Count;
        public void Bind(Crawler c)
        {
            Crawler = c;
        }
        /// <summary>
        /// 全局使用的头
        /// </summary>
        private readonly WebHeaderCollection _headers = new WebHeaderCollection();


        public void AddHeader(string key, string value)
        {
            _headers.Add(key, value);
        }

        public void AddCookie(CookieCollection cc)
        {
            _cookies.Add(cc);
        }

        private readonly CookieContainer _cookies = new CookieContainer();
        public void AddCookie(string key, string value, string domain = null)
        {
            if (domain == null)
                foreach (var d in Crawler.Config.Domains)
                {
                    _cookies.Add(new Cookie(key, value, "/", d));
                }
            else
                _cookies.Add(new Cookie(key, value, "/", domain));



        }

        public string GetCookie(string name, string domain)
        {
            var cs = _cookies.GetCookies(new Uri("http://"+domain));
            return cs[name].Value;
        }

        public void AddUrl(string url, Options options = null)
        {
            if (options == null)
            {
                options = new Options();//使用默认值
            }

            var r = new Request
            {
                Method = options.Method,
                URL = url,
                UserAgent = Crawler.Config.UserAgent,
                Timeout = Crawler.Config.Timeout,
                Postdata = options.Data,
                Header = new WebHeaderCollection
                {
                    _headers,
                    options.Headers
                },
                CookieCollection = _cookies.GetCookies(new Uri(url)),


            };



            _queue.Enqueue(r);
        }

        public void AddUrls(IEnumerable<string> urls, Options os = null)
        {
            foreach (var u in urls)
            {
                AddUrl(u, os);
            }
        }

        public void AddScanUrl(string url, Options options = null)
        {
            throw new NotImplementedException();
        }

        public string RequestUrl(string url, Options options = null)
        {
            throw new NotImplementedException();
        }

        public void SetUserAgent(string userAgent)
        {
            throw new NotImplementedException();
        }

        public void SetCharset(string charset)
        {
            throw new NotImplementedException();
        }
    }
}