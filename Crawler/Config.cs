using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace CrawlerDotNet.Core
{
    /// <summary>
    /// 配置类
    /// </summary>
    public class Config
    {
        public Config() { }

        public Config(string jsonText)
        {
            //从json文本中读取配置

        }


        public static Config GetConfigFormPath(string path)
        {
            return GetConfigFormJsonString(File.ReadAllText(path));
        }
        public static Config GetConfigFormJsonString(string jsonText)
        {
            return JsonConvert.DeserializeObject<Config>(jsonText);
        }
        //todo 是不是要放到别的地方?
        public string MysqlConString { get; set; } = "Data Source='localhost';User Id='root';Password='123456';";
        public bool ChangeProxyEveryPage => false;
        public bool EnableJs => true;
        public string Name { get; set; } = "crawler";



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
        [JsonConverter(typeof(StringEnumConverter))]
        public RepeatWhenEver RepeatWhen { get; set; } = RepeatWhenEver.hour;
        [JsonConverter(typeof(ChinaDateTimeConverter))]
        public TimeSpan RepeatAt { get; set; } = new TimeSpan(0, 0, 0, 5);
        public int Timeout { get; set; } = 5000;
        public string UserAgent { get; set; } = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
        /// <summary>
        /// 单个HTTP请求失败时, 可自动重试. 通过tryTimes设置单个HTTP请求的最多重复请求次数
        /// 默认值是0, 0和1都表示单个HTTP请求最多可请求一次
        /// 请求失败的情况包括: “请求过程出现异常(如, 请求超时)”和”请求返回内容为空”
        /// </summary>
        public int TryTimes { get; set; } = 10;
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

        public override string ToString()
        {
            var r = "\r\n*******************************配置***********************************\r\n";
            r += "名字:" + Name + "\r\n";
            r += "启动页:" + ScanUrls + "\r\n";
            r += "运行方式:" + RepeatWhen + "\r\n";
            r += "运行时间:" + RepeatAt + "\r\n";
            r += "线程数:" + ThreadNum + "\r\n";
            r += "抽取项:" + ScanUrls + "\r\n";
            r += "**********************************************************************";
            return r;
        }
    }

    public class Field
    {
        private JObject _mysqlKeyWordjson;
        public Field()
        {
            _mysqlKeyWordjson = JObject.Parse(File.ReadAllText("tomysql.json"));
        }

        public override string ToString()
        {
            return $"name:{Name},Selector:{Selector}";
        }

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
        [JsonConverter(typeof(StringEnumConverter))]
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
        [JsonConverter(typeof(StringEnumConverter))]
        public FieldType Type { get; set; } = FieldType.String;
        [JsonIgnore]
        public string SqlType => _mysqlKeyWordjson[Type.ToString()].Value<string>().ToUpper();

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
        [JsonConverter(typeof(StringEnumConverter))]
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

    public class ChinaDateTimeConverter : DateTimeConverterBase
    {
        private static IsoDateTimeConverter dtConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return dtConverter.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            dtConverter.WriteJson(writer, value, serializer);
        }
    }

    public enum FieldType
    {
        Int,
        Float,
        Time,
        String,
        Bool,

    }
    public enum RepeatWhenEver
    {
        once,
        min,
        hour,
        day,
        week,
        month
    }

    public enum SelectorType
    {
        JsonPath,
        XPath,
        Regex,
        StringGetMid
    }

    public enum SourceType
    {
        Page, //表示从当前内容页的网页内容中抽取数据
        AttachedUrl,//后可以发起一个新的HTTP请求, 然后从HTTP请求返回的数据中抽取数据
        UrlContext,//表示从内容页附加数据中抽取数据 内容页附加数据, 可以是任意一段字符串, 例如, HTML代码. 一般用法是, 将列表页中的数据附加到内容页中, 以便在抽取内容页数据时, 可以从中抽取列表页中的数据.
    }
}