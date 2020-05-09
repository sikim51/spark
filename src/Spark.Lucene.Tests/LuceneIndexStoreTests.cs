using System;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spark.Engine.Search.Model;
using Spark.Lucene.Indexer;

namespace Spark.Lucene.Tests
{
    [TestClass]
    public class LuceneIndexStoreTests
    {

        private IndexWriter indexWriter;
        private IndexReader indexReader;

        [TestInitialize]
        public void Initialize()
        {
            var directory = new RAMDirectory();

            //create an analyzer to process the text
            Analyzer analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);
            IndexWriterConfig config = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
            indexWriter = new IndexWriter(directory, config);

            indexReader = indexWriter.GetReader(false);
        }

        [TestMethod]
        public void ShouldClean()
        {
            var indexStore = new LuceneIndexStore(indexWriter, indexReader);
            indexStore.Clean();
        }

        [TestMethod]
        public void ShouldSaveDocument()
        {
            var indexStore = new LuceneIndexStore(indexWriter, indexReader);
            var document = new Document();
            document.Add(new StoredField(IndexFieldNames.ID, "id"));
            indexStore.Save(document);
        }
    }
}
