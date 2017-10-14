using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;

namespace Crawler.Core.Pipeline
{
    public interface IPipeline
    {
        Logger Logger { get; }
        void Handle(Page p);
    }

    public abstract class BasePipeline : IPipeline
    {
        public Logger Logger => Crawler.Logger;
        public virtual void Handle(Page p) { }
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
        public override void Handle(Page p)
        {
            var doc = new BsonDocument(p.Results);
            _collection.InsertOne(doc);
        }
    }
}