using MongoDB.Bson;
using MongoDB.Driver;
using Spark.Engine.Core;
using System;
using Spark.Store.Mongo;
using Spark.Engine.Model;
using Spark.Mongo.Search.Indexer;
using Spark.Engine.Store.Interfaces;
using Spark.Engine.Search.Model;

namespace Spark.Mongo.Search.Common
{
    public class MongoIndexStore : IIndexStore
    {
        private IMongoDatabase _database;
        private MongoIndexMapper _indexMapper;
        public IMongoCollection<BsonDocument> Collection;

        public MongoIndexStore(string mongoUrl, MongoIndexMapper indexMapper)
        {
            _database = MongoDatabaseFactory.GetMongoDatabase(mongoUrl);
            _indexMapper = indexMapper; 
            Collection = _database.GetCollection<BsonDocument>(Config.MONGOINDEXCOLLECTION);
        }

        public void Save(IndexValue indexValue)
        {
            var result = _indexMapper.MapEntry(indexValue);

            foreach (var doc in result)
            {
                Save(doc);
            }
        }

        public void Delete(Entry entry)
        {
            string id = entry.Key.WithoutVersion().ToOperationPath();
            var query = Builders<BsonDocument>.Filter.Eq(IndexFieldNames.ID, id);
            Collection.DeleteMany(query);
        }

        public void Clean()
        {
            Collection.DeleteMany(Builders<BsonDocument>.Filter.Empty);
        }

        public void Save<T>(T document)
        {
            var docu = document as BsonDocument;
            try
            {
                string keyvalue = docu.GetValue(IndexFieldNames.ID).ToString();
                var query = Builders<BsonDocument>.Filter.Eq(IndexFieldNames.ID, keyvalue);

                // todo: should use Update: collection.Update();
                Collection.DeleteMany(query);
                Collection.InsertOne(docu);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}
