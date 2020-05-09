using System;
using Hl7.Fhir.Model;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spark.Engine.Core;
using Spark.Engine.Search.Common;
using Spark.Lucene.Indexer;

namespace Spark.Lucene.Tests
{
    [TestClass]
    public class LuceneIndexerTests
    {
        private IndexWriter indexWriter;
        private IndexReader indexReader;
        private Key _fakeKey = new Key("http://localhost", "type", "resourceid", "versionid");

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
        public void ShouldIndexResource()
        {
            var store = new LuceneIndexStore(indexWriter, indexReader);
            var definitions = new Definitions();
            definitions.Add(new Definition() { Resource = nameof(Person), Query = new ElementQuery() });
            var indexer = new LuceneIndexer(store, definitions);
            var personResource = new Person();

            var entry = Entry.Create(Hl7.Fhir.Model.Bundle.HTTPVerb.GET, _fakeKey, personResource);

            indexer.Process(entry);
        }

        [TestMethod]
        public void ShouldDeleteIndexedResource()
        {
            var store = new LuceneIndexStore(indexWriter, indexReader);
            var definitions = new Definitions();
            definitions.Add(new Definition() { Resource = nameof(Person), Query = new ElementQuery() });
            var indexer = new LuceneIndexer(store, definitions);
            var personResource = new Person();

            var entry = Entry.Create(Hl7.Fhir.Model.Bundle.HTTPVerb.GET, _fakeKey, personResource);
            indexer.Process(entry);

            var deleteEntry = Entry.DELETE(_fakeKey, DateTimeOffset.Now);
            indexer.Process(deleteEntry);
        }
    }
}
