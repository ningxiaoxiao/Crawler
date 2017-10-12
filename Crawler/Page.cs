namespace Crawler.Core
{
    /// <summary>
    /// 表示当前正在爬取的网页对象
    /// </summary>
    public class Page
    {

        public Page(string raw)
        {
            Raw = raw;
        }

        /// <summary>
        /// 当前正在爬取的网页url
        /// </summary>
        string Url { get;  }
        /// <summary>
        /// 当前网页的内容. 通过自动JS渲染的网页, page.raw是JS渲染后的网页内容
        /// </summary>
        public string Raw { get; }
        /// <summary>
        /// 请求page网页时使用的请求参数等数据
        /// </summary>
        System.Net.HttpRequestHeader Request { get;}
        /// <summary>
        /// 请求page网页返回的响应信息
        /// </summary>
        System.Net.HttpWebResponse Response { get; }
        /// <summary>
        /// 当前网页的附加数据, 是开发者自定义的一段代码, 例如, HTML代码.
        /// </summary>
        string contextData { get; }

        /// <summary>
        /// 只能在afterExtractField回调函数中使用, 用来过滤抽取项中不需要的抽取结果, 被过滤的抽取结果不会被保存到数据库中
        /// </summary>
        /// <param name="fieldName">要过滤的抽取项名字, 必须是包含不需要的抽取结果的对象数组, String类型, 可不填, 无默认值. 如果不传入参数, 表示过滤当前网页的所有抽取结果; 如果传入参数, 表示过滤该抽取项的当前抽取结果</param>
        void Skip(string fieldName = null) { }

        
    }
}