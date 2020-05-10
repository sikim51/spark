using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Rest;
using Spark.Core;
using Spark.Engine.Core;
using Spark.Engine.Store.Interfaces;

namespace Spark.Lucene.Indexer
{
    public class LuceneIndex : IFhirIndex
    {
        private LuceneIndexer _indexer;
        private readonly LuceneSearcher _luceneSearcher;
        private IIndexStore _indexStore;

        public LuceneIndex(IIndexStore indexStore, LuceneIndexer indexer, LuceneSearcher luceneSearcher)
        {
            _indexStore = indexStore;
            _indexer = indexer;
            _luceneSearcher = luceneSearcher;
        }

        public void Clean()
        {
            _indexStore.Clean();
        }

        public Key FindSingle(string resource, SearchParams searchCommand)
        {
            SearchResults results = Search(resource, searchCommand);
            if (results.Count > 1)
            {
                throw Error.BadRequest("The search for a single resource yielded more than one.");
            }
            else if (results.Count == 0)
            {
                throw Error.BadRequest("No resources were found while searching for a single resource.");
            }
            else
            {
                string location = results.FirstOrDefault();
                return Key.ParseOperationPath(location);
            }
        }

        public SearchResults GetReverseIncludes(IList<IKey> keys, IList<string> revIncludes)
        {
            return _luceneSearcher.GetReverseIncludes(keys, revIncludes);
        }

        public void Process(IEnumerable<Entry> entries)
        {
            foreach (var i in entries)
            {
                Process(i);
            }
        }

        public void Process(Entry entry)
        {
            _indexer.Process(entry);
        }

        public SearchResults Search(string resource, SearchParams searchCommand)
        {
            return  _luceneSearcher.Search(resource, searchCommand);
        }
    }
}
