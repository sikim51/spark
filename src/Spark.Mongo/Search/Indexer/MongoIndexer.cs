/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.Collections.Generic;

using Hl7.Fhir.Model;
using MongoDB.Bson;
using Spark.Engine.Core;
using Spark.Engine.Extensions;
using Spark.Engine.Search.Common;
using Spark.Engine.Search.Indexer;
using Spark.Engine.Store.Interfaces;
using Spark.Mongo.Search.Indexer;

namespace Spark.Mongo.Search.Common
{

    public class MongoIndexer : FhirIndexer<MongoIndexStore>
    {

        public MongoIndexer(IIndexStore store, Definitions definitions): base(store, definitions)
        {
        }

        protected override void put(IKey key, int level, DomainResource resource)
        {
            BsonIndexDocumentBuilder builder = new BsonIndexDocumentBuilder(key);
            builder.WriteMetaData(key, level, resource);

            var matches = Definitions.MatchesFor(resource);
            foreach (Definition definition in matches)
            {
                definition.Harvest(resource, builder.InvokeWrite);
            }

            BsonDocument document = builder.ToDocument();

            Store.Save(document);
        }
    }
}