using System;
using System.Text.RegularExpressions;

namespace Crawler.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class Config
    {
        public const string UserAgentAndroid = "";
        public const string UserAgentIos = "";
        public const string UserAgentComputer = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
        public const string UserAgentMobile = "";

        public bool ChangeProxyEveryPage => false;
        public bool EnableJs => true;
        private string[] _domains;
        /// <summary>
        /// 可以发起请求的合法域,默认是scanurls的host
        /// </summary>
        public string[] Domains
        {
            get { return _domains ?? new[] { new Uri(ScanUrls).Host }; }
            set { _domains = value; }
        }

        /// <summary>
        /// 起始页
        /// </summary>
        public string ScanUrls { get; set; }
        /// <summary>
        /// 识别内容页正则
        /// </summary>
        public Regex ContentUrlRegexes { get; set; } = new Regex("http");
        /// <summary>
        /// 列表页正则
        /// </summary>
        public Regex HelperUrlRegexes { get; set; } = new Regex("http");
        /// <summary>
        /// 抽取项
        /// </summary>
        public Field[] Fields { get; set; }
        /// <summary>
        /// 间隔 秒 默认30分钟
        /// </summary>
        public int Interval { get; set; } = 1800;
        //todo 设置爬取的类型 定时 定期 重复 一次爬取
        public int Timeout { get; set; } = 1000;
        public string UserAgent { get; set; } = UserAgentComputer;
        /// <summary>
        /// 单个HTTP请求失败时, 可自动重试. 通过tryTimes设置单个HTTP请求的最多重复请求次数
        /// 默认值是0, 0和1都表示单个HTTP请求最多可请求一次
        /// 请求失败的情况包括: “请求过程出现异常(如, 请求超时)”和”请求返回内容为空”
        /// </summary>
        public int TryTimes { get; set; } = 3;
        /// <summary>
        /// 设置应用是否优先爬取待爬队列中的scanUrl
        /// 默认值是false, 神箭手应用会按顺序依次爬取待爬队列中的url, 包括helperUrl, contentUrl和scanUrl
        /// </summary>
        public bool EntriesFirst { get; set; } = false;
        /// <summary>
        /// 同时工作的线程数 默认10
        /// </summary>
        public int ThreadNum { get; set; } = 10;
        /// <summary>
        /// 启动页是0
        /// </summary>
        public int MaxDeth { get; set; } = 2;
    }

    public class Field
    {
        /// <summary>
        /// 给抽取项起个名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 给抽取项起个别名(建议用汉字)
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// 设置抽取规则的类型  默认值是SelectorType.XPath
        /// </summary>
        public SelectorType Selectortype { get; set; } = SelectorType.XPath;
        /// <summary>
        /// 定义抽取规则
        /// </summary>
        public string Selector { get; set; }
        /// <summary>
        /// 设置抽取项的值是否不可为空 
        /// 默认值是false
        /// 值设为true, 表示如果抽取项的值为空, 则该条爬取结果会被过滤掉, 不会存入数据库中
        /// </summary>
        public bool Required { get; set; } = false;
        /// <summary>
        /// 设置抽取项中每条抽取结果的数据类型
        /// 默认值是string类型
        /// </summary>
        public Type Type { get; set; } = typeof(string);
        /// <summary>
        /// 设置抽取项是否是临时的(临时的抽取项不会保存到爬取结果中)
        /// </summary>
        public bool Transient { get; set; } = false;
        /// <summary>
        /// 抽取项的数据来源并不仅限于当前内容页的网页内容, 
        /// 通过设置sourceType, 
        /// 不仅可以使抽取项从当前内容页的网页内容中抽取数据, 
        /// 还可以从”异步请求返回的数据”或”内容页附加数据”中抽取数据
        /// </summary>
        public SourceType SourceType { get; set; } = SourceType.Page;
        /// <summary>
        /// 设置抽取项的子抽取项
        /// </summary>
        public Field[] Childrens { get; set; }
        /// <summary>
        /// 设置是否将抽取项作为去重字段
        /// 默认值是false
        /// </summary>
        public bool PrimaryKey { get; set; } = false;
        /// <summary>
        /// 设置抽取项是否同时抽取多条数据
        /// 默认值是false
        /// 值设为true, 表示抽取项同时抽取多条数据, 抽取项的值是数组类型
        /// </summary>
        public bool Repeated { get; set; } = false;


    }

    public enum SelectorType
    {
        JsonPath,
        XPath,
        Regex
    }

    public enum SourceType
    {
        Page, //表示从当前内容页的网页内容中抽取数据
        AttachedUrl,//后可以发起一个新的HTTP请求, 然后从HTTP请求返回的数据中抽取数据
        UrlContext,//表示从内容页附加数据中抽取数据 内容页附加数据, 可以是任意一段字符串, 例如, HTML代码. 一般用法是, 将列表页中的数据附加到内容页中, 以便在抽取内容页数据时, 可以从中抽取列表页中的数据.
    }
}