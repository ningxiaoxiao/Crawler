using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MySql.Data.MySqlClient;
using NLog;

namespace Crawler.Core.Pipeline
{
    public interface IPipeline
    {
        Config Config { get; }
        Logger Logger { get; }
        void Handle(Page p);
    }

    public abstract class BasePipeline : IPipeline
    {
        public Logger Logger => Crawler.Logger;
        public Config Config => Crawler.Config;

        public void Handle(Page p)
        {
            //过虑一次
            if (p.Results.Count == 0)
                return;
            OnHandel(p);

        }
        public virtual void OnHandel(Page p) { }
    }

    public class MySqlPipline : BasePipeline
    {
        private string sqlconstring;


        public MySqlPipline(string sqlcon = "Data Source='localhost';User Id='root';Password='1234';charset='utf8';pooling=true")
        {
            sqlconstring = sqlcon;


            var mySqlConnection = new MySqlConnection(sqlconstring);
            mySqlConnection.Open();
            try
            {
                var cmd = new MySqlCommand("CREATE DATABASE " + DatabaseName, mySqlConnection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            mySqlConnection.ChangeDatabase(DatabaseName);
            try
            {
                var cols = string.Empty;

                foreach (var field in Config.Fields)
                {
                    //todo 数据类型自动翻译
                    cols += $"{field.Name} {field.Type.ToString().Replace("System.", "").Replace("String", "text").ToUpper()} ,";
                }

                var cmd = new MySqlCommand($"CREATE TABLE {DataTableName} (id INT NOT NULL AUTO_INCREMENT,timestamp TIMESTAMP,{cols + "PRIMARY KEY (id)"})", mySqlConnection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }



        }

        private const string DatabaseName = "crawler";
        private const string DataTableName = "result";

        public override void OnHandel(Page p)
        {
            var con = new MySqlConnection(sqlconstring);
            con.Open();
            con.ChangeDatabase(DatabaseName);
            var cmd = new MySqlCommand { Connection = con };

            //建立文本

            var keys = "timestamp";
            var values = "\"" + p.Timestamp + "\"";

            foreach (var result in p.Results)
            {
                keys += $",{result.Key}";
                values += $",\"{result.Value}\"";
            }
            try
            {
                cmd.CommandText = $"INSERT INTO {DataTableName}({keys}) VALUES({values})";
                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Logger.Error("sql保存错误:" + e.Message);
            }

        }
    }

    public class MangoDBPipline : BasePipeline
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public MangoDBPipline()
        {
            //建立连接
            var client = new MongoClient();
            //建立数据库
            var database = client.GetDatabase("TestDb");
            //建立collection
            _collection = database.GetCollection<BsonDocument>("results");
        }
        public override void OnHandel(Page p)
        {

            Logger.Info("开始保存");
            var doc = new BsonDocument();
            foreach (var r in p.Results)
            {
                if (!r.Skip)
                    doc.Add(r.Key, r.Value);
            }
            Logger.Info("保存数量:" + doc.ElementCount);
            try
            {
                if (doc.ElementCount > 0)
                    _collection.InsertOne(doc);
            }
            catch (Exception e)
            {
                Logger.Debug(e, "保存出现问题");
            }

            Logger.Info("保存结束");
        }
    }
}