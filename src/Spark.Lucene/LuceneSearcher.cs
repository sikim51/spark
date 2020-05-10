using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Rest;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Spark.Engine.Core;
using Spark.Engine.Search;
using Spark.Engine.Search.Model;
using Spark.Search;
using SM = Spark.Engine.Search.Model;

namespace Spark.Lucene
{
    public class LuceneSearcher : FhirSearcher
    {
        private readonly LuceneIndexStore _luceneIndexStore;
        private readonly IndexSearcher _searcher;

        public LuceneSearcher(LuceneIndexStore luceneIndexStore, ILocalhost localhost, IFhirModel fhirModel) : base(localhost, fhirModel)
        {
            _luceneIndexStore = luceneIndexStore;
            _searcher = new SearcherFactory().NewSearcher(luceneIndexStore.IndexReader);
        }

        public override SearchResults GetReverseIncludes(IList<IKey> keys, IList<string> revIncludes)
        {

            SearchResults results = new SearchResults();

            if (keys != null && revIncludes != null)
            {
                var riQueries = new List<Query>();

                foreach (var revInclude in revIncludes)
                {
                    var ri = SM.ReverseInclude.Parse(revInclude);
                    if (!ri.SearchPath.Contains(".")) //for now, leave out support for chained revIncludes. There aren't that many anyway.
                    {

                        var internalIdsQuery = new BooleanQuery();
                        foreach (var key in keys)
                        {
                            internalIdsQuery.Add(new BooleanClause(new TermQuery(new Term(ri.SearchPath, $"{key.TypeName}/{key.ResourceId}")), Occur.SHOULD));
                        }

                        riQueries.Add(
                            new BooleanQuery()
                            {
                               new BooleanClause(new TermQuery(new Term(IndexFieldNames.RESOURCE,ri.ResourceType)), Occur.MUST),
                               new BooleanClause(internalIdsQuery, Occur.MUST)
                              });
                    }
                }

                if (riQueries.Count > 0)
                {
                    var query = new BooleanQuery();
                    foreach (var q in riQueries)
                    {
                        query.Add(new BooleanClause(q, Occur.SHOULD));
                    }
                    var selfLinks = CollectSelfLinks(query);
                    var searchResult = new SearchResults()
                    {
                        MatchCount = selfLinks.Count
                    };
                    searchResult.AddRange(selfLinks);
                    return searchResult;
                }
            }
            return results;
        }

        protected override List<string> OnCollectKeys(string resourceType, IEnumerable<Criterium> criteria, int level = 0)
        {
            throw new NotImplementedException();
        }

        private List<string> CollectKeys(Query query)
        {
            var results = _searcher.Search(query, -1).ScoreDocs;
            if (results.Count() > 0)
                return results.Select(res => _searcher.Doc(res.Doc).Get(IndexFieldNames.ID)).ToList();

            return new List<string>();
        }

        private List<string> CollectSelfLinks(Query query)
        {
            var results = _searcher.Search(query, -1).ScoreDocs;
            if (results.Count() > 0)
                return results.Select(res => _searcher.Doc(res.Doc).Get(IndexFieldNames.SELFLINK)).ToList();

            return new List<string>();
        }

        protected override IEnumerable<string> OnCollectSelfLinks(string resourceType, IEnumerable<Criterium> criteria, SearchResults results, int level, IList<Tuple<string, SortOrder>> sortItems)
        {
            throw new NotImplementedException();
        }
    }
}
