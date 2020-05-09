/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;

//using Hl7.Fhir.Support;
using MongoDB.Bson;
using MongoDB.Driver;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using MongoDB.Driver.Core.Configuration;
using Spark.Engine.Core;
using Spark.Mongo.Search.Common;
using Spark.Engine.Extensions;
using SM = Spark.Engine.Search.Model;
using Spark.Engine.Search;
using Spark.Engine.Search.Model;

namespace Spark.Search.Mongo
{

    public class MongoSearcher : FhirSearcher
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public MongoSearcher(MongoIndexStore mongoIndexStore, ILocalhost localhost, IFhirModel fhirModel) : base(localhost, fhirModel)
        {
            _collection = mongoIndexStore.Collection;
        }

        private List<BsonValue> CollectKeys(FilterDefinition<BsonDocument> query)
        {
            var cursor = _collection.Find(query)
                .Project(Builders<BsonDocument>.Projection.Include(IndexFieldNames.ID))
                .ToEnumerable();
            if (cursor.Count() > 0)
                return cursor.Select(doc => doc.GetValue(IndexFieldNames.ID)).ToList();
            return new List<BsonValue>();
        }

        private List<BsonValue> CollectSelfLinks(FilterDefinition<BsonDocument> query, SortDefinition<BsonDocument> sortBy)
        {
            var cursor = _collection.Find(query);

            if (sortBy != null)
            {
                cursor.Sort(sortBy);
            }

            cursor = cursor.Project(Builders<BsonDocument>.Projection.Include(IndexFieldNames.SELFLINK));

            return cursor.ToEnumerable().Select(doc => doc.GetValue(IndexFieldNames.SELFLINK)).ToList();
        }

        private SearchResults KeysToSearchResults(IEnumerable<BsonValue> keys)
        {
            var results = new SearchResults();

            if (keys.Count() > 0)
            {
                var cursor = _collection.Find(Builders<BsonDocument>.Filter.In(IndexFieldNames.ID, keys))
                    .Project(Builders<BsonDocument>.Projection.Include(IndexFieldNames.SELFLINK))
                    .ToEnumerable();

                foreach (BsonDocument document in cursor)
                {
                    string id = document.GetValue(IndexFieldNames.SELFLINK).ToString();
                    //Uri rid = new Uri(id, UriKind.Relative); // NB. these MUST be relative paths. If not, the data at time of input was wrong 
                    results.Add(id);
                }
                results.MatchCount = results.Count();
            }
            return results;
        }

        protected override List<string> OnCollectKeys(string resourceType, IEnumerable<Criterium> criteria, int level = 0)
        {

            var keys = CollectKeys(resourceType, criteria, null, level);
            return keys.Select(k => k.ToString()).ToList();
        }

        private List<BsonValue> CollectKeys(string resourceType, IEnumerable<Criterium> criteria, SearchResults results, int level)
        {
            Dictionary<Criterium, Criterium> closedCriteria = CloseChainedCriteria(resourceType, criteria, results, level);

            //All chained criteria are 'closed' or 'rolled up' to something like subject IN (id1, id2, id3), so now we AND them with the rest of the criteria.
            FilterDefinition<BsonDocument> resultQuery = CreateMongoQuery(resourceType, results, level, closedCriteria);

            return CollectKeys(resultQuery);
        }

        protected override IEnumerable<string> OnCollectSelfLinks(string resourceType, IEnumerable<Criterium> criteria, SearchResults results, int level, IList<Tuple<string, SortOrder>> sortItems)
        {
            var selfLinks = CollectSelfLinks(resourceType, criteria, results, level, sortItems);

            foreach (BsonValue selfLink in selfLinks)
            {
                yield return selfLink.ToString();
            }
        }

        private List<BsonValue> CollectSelfLinks(string resourceType, IEnumerable<Criterium> criteria, SearchResults results, int level, IList<Tuple<string, SortOrder>> sortItems )
        {
            Dictionary<Criterium, Criterium> closedCriteria = CloseChainedCriteria(resourceType, criteria, results, level);

            //All chained criteria are 'closed' or 'rolled up' to something like subject IN (id1, id2, id3), so now we AND them with the rest of the criteria.
            FilterDefinition<BsonDocument> resultQuery = CreateMongoQuery(resourceType, results, level, closedCriteria);
            SortDefinition<BsonDocument> sortBy = CreateSortBy(sortItems);
            return CollectSelfLinks(resultQuery, sortBy);
        }

        private static SortDefinition<BsonDocument> CreateSortBy(IList<Tuple<string, SortOrder>> sortItems)
        {
            if (sortItems.Any() == false)
                return null;

            SortDefinition<BsonDocument> sortDefinition = null;
            var first = sortItems.FirstOrDefault();
            if (first.Item2 == SortOrder.Ascending)
            {
                sortDefinition = Builders<BsonDocument>.Sort.Ascending(first.Item1);
            }
            else
            {
                sortDefinition = Builders<BsonDocument>.Sort.Descending(first.Item1);
            }
            sortItems.Remove(first);
            foreach (Tuple<string, SortOrder> sortItem in sortItems)
            {
                if (sortItem.Item2 == SortOrder.Ascending)
                {
                    sortDefinition = sortDefinition.Ascending(sortItem.Item1);
                }
                else
                {
                    sortDefinition = sortDefinition.Descending(sortItem.Item1);
                }
            }
            return sortDefinition;

        }

        private static FilterDefinition<BsonDocument> CreateMongoQuery(string resourceType, SearchResults results, int level, Dictionary<Criterium, Criterium> closedCriteria)
        {
            FilterDefinition<BsonDocument> resultQuery = CriteriaMongoExtensions.ResourceFilter(resourceType, level);
            if (closedCriteria.Count() > 0)
            {
                var criteriaQueries = new List<FilterDefinition<BsonDocument>>();
                foreach (var crit in closedCriteria)
                {
                    if (crit.Value != null)
                    {
                        try
                        {
                            criteriaQueries.Add(crit.Value.ToFilter(resourceType));
                        }
                        catch (ArgumentException ex)
                        {
                            if (results == null) throw; //The exception *will* be caught on the highest level.
                            results.AddIssue(String.Format("Parameter [{0}] was ignored for the reason: {1}.", crit.Key.ToString(), ex.Message), OperationOutcome.IssueSeverity.Warning);
                            results.UsedCriteria.Remove(crit.Key);
                        }
                    }
                }
                if (criteriaQueries.Count > 0)
                {
                    FilterDefinition<BsonDocument> criteriaQuery = Builders<BsonDocument>.Filter.And(criteriaQueries);
                    resultQuery = Builders<BsonDocument>.Filter.And(resultQuery, criteriaQuery);
                }
            }

            return resultQuery;
        }
        

        public SearchResults GetReverseIncludes(IList<IKey> keys, IList<string> revIncludes)
        {
            BsonValue[] internal_ids = keys.Select(k => BsonString.Create(String.Format("{0}/{1}", k.TypeName, k.ResourceId))).ToArray();

            SearchResults results = new SearchResults();

            if (keys != null && revIncludes != null)
            {
                var riQueries = new List<FilterDefinition<BsonDocument>>();

                foreach (var revInclude in revIncludes)
                {
                    var ri = SM.ReverseInclude.Parse(revInclude);
                    if (!ri.SearchPath.Contains(".")) //for now, leave out support for chained revIncludes. There aren't that many anyway.
                    {
                        riQueries.Add(
                            Builders<BsonDocument>.Filter.And(
                                Builders<BsonDocument>.Filter.Eq(IndexFieldNames.RESOURCE, ri.ResourceType)
                                , Builders<BsonDocument>.Filter.In(ri.SearchPath, internal_ids)));
                    }
                }

                if (riQueries.Count > 0)
                {
                    var revIncludeQuery = Builders<BsonDocument>.Filter.Or(riQueries);
                    var resultKeys = CollectKeys(revIncludeQuery);
                    results = KeysToSearchResults(resultKeys);
                }
            }
            return results;
        }

       

        //TODO: Delete, F.Query is obsolete.
        /*
        public SearchResults Search(F.Query query)
        {
            SearchResults results = new SearchResults();

            var criteria = parseCriteria(query, results);

            if (!results.HasErrors)
            {
                results.UsedCriteria = criteria;
                //TODO: ResourceType.ToString() sufficient, or need to use EnumMapping?
                var normalizedCriteria = NormalizeNonChainedReferenceCriteria(criteria, query.ResourceType.ToString());
                List<BsonValue> keys = CollectKeys(query.ResourceType.ToString(), normalizedCriteria, results);

                int numMatches = keys.Count();

                results.AddRange(KeysToSearchResults(keys));
                results.MatchCount = numMatches;
            }

            return results;
        }
        */



        //TODO: Delete, F.Query is obsolete.
        /*
        private List<Criterium> parseCriteria(F.Query query, SearchResults results)
        {
            var result = new List<Criterium>();
            foreach (var c in query.Criteria)
            {
                try
                {
                    result.Add(Criterium.Parse(c));
                }
                catch (Exception ex)
                {
                    results.AddIssue(String.Format("Could not parse parameter [{0}] for reason [{1}].", c.ToString(), ex.Message));
                }
            }
            return result;
        }
         */
    }
}