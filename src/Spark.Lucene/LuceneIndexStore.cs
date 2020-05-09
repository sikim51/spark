using System;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Spark.Engine.Core;
using Spark.Engine.Model;
using Spark.Engine.Search.Model;
using Spark.Engine.Store.Interfaces;

namespace Spark.Lucene
{
    public class LuceneIndexStore : IIndexStore
    {
        private readonly IndexWriter _indexWriter;
        private readonly IndexReader _indexReader;

        public LuceneIndexStore(IndexWriter indexWriter, IndexReader indexReader)
        {
            _indexWriter = indexWriter;
            _indexReader = indexReader;
        }

        public void Clean()
        {
            _indexWriter.DeleteAll();
            _indexWriter.Commit();
        }

        public void Delete(Entry entry)
        {
            string id = entry.Key.WithoutVersion().ToOperationPath();
            _indexWriter.DeleteDocuments(new Term(IndexFieldNames.ID, id));
            _indexWriter.Commit();
        }

        public void Save(IndexValue indexValue)
        {
            throw new NotImplementedException();
        }

        public void Save<T>(T document)
        {
            var docu = document as Document;

            if (docu != null)
            {
                _indexWriter.UpdateDocument(new Term(IndexFieldNames.ID, docu.GetField(IndexFieldNames.ID).GetStringValue()), docu);
                _indexWriter.Commit();
            }
        }
    }
}
