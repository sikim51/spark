using System;
using Hl7.Fhir.Model;
using Lucene.Net.Documents;
using Spark.Engine.Core;
using Spark.Engine.Search.Common;
using Spark.Engine.Search.Indexer;

namespace Spark.Lucene.Indexer
{
    public class LuceneIndexer : FhirIndexer<LuceneIndexStore>
    {
        public LuceneIndexer(LuceneIndexStore store, Definitions definitions) : base(store, definitions)
        {
        }

        protected override void put(IKey key, int level, DomainResource resource)
        {
            LuceneIndexDocumentBuilder builder = new LuceneIndexDocumentBuilder(key);
            builder.WriteMetaData(key, level, resource);

            var matches = Definitions.MatchesFor(resource);
            foreach (Definition definition in matches)
            {
                definition.Harvest(resource, builder.InvokeWrite);
            }

            Document document = builder.ToDocument();

            Store.Save(document);
        }
    }
}
