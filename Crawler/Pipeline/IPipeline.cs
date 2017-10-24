using System;
using System.Linq;
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

        public const string DatabaseName = "crawler";
        public string DataTableName => Config.Name + "ExtractResults";

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
        private readonly string _sqlConString;



        public MySqlPipline(string sqlcon = "Data Source='localhost';User Id='root';Password='123456';charset='utf8';pooling=true")
        {
            _sqlConString = sqlcon;

            var mySqlConnection = new MySqlConnection(_sqlConString);
            try
            {
                mySqlConnection.Open();
            }
            catch (Exception e)
            {
                throw e;
            }


            try
            {
                mySqlConnection.ChangeDatabase(DatabaseName);
            }
            catch
            {
                Logger.Info("更改数据库到[" + DatabaseName + "]失败,开始新建数据库");
                try
                {
                    var c = new MySqlCommand("CREATE DATABASE " + DatabaseName, mySqlConnection);
                    c.ExecuteNonQuery();
                }
                catch
                {
                    throw new Exception("无法创建数据库");
                }
                mySqlConnection.ChangeDatabase(DatabaseName);
                Logger.Info("自动新建数据库完成:" + DatabaseName);
            }
            var cmd = new MySqlCommand("show tables", mySqlConnection);
            var tabels = cmd.ExecuteReader();
            var haveTable = false;
            while (tabels.Read())
            {
                var tablename = tabels.GetString(0);
                if (tablename != DataTableName) continue;
                haveTable = true;
                break;
            }
            tabels.Close();
            if (haveTable) return;
            try
            {
                Logger.Info("没有表[" + DataTableName + "]新建中...");
                if (Config.Fields == null || Config.Fields.Length == 0)
                {
                    Logger.Warn("没有抽取项,没有新建表");
                    return;
                }
                var cols = Config.Fields.Aggregate(string.Empty, (current, field) => current + $"{field.Name} {field.SqlType} ,");

                cmd = new MySqlCommand($"CREATE TABLE {DataTableName} (id INT NOT NULL AUTO_INCREMENT,timestamp TIMESTAMP,{cols + "PRIMARY KEY (id)"})", mySqlConnection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {

                throw new Exception("表[" + DataTableName + "]新建失败:\r\n" + e);
            }

            Logger.Info("表[" + DataTableName + "]新建成功");

        }

        public override void OnHandel(Page p)
        {
            var con = new MySqlConnection(_sqlConString);
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

    public class MangoPipline : BasePipeline
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public MangoPipline()
        {
            //建立连接
            var client = new MongoClient();
            //建立数据库
            var database = client.GetDatabase("crawler");
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
                Logger.Error(e, "保存出现问题");
            }

            Logger.Info("保存结束");
        }
    }
}