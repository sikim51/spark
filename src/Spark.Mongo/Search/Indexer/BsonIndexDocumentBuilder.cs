using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Hl7.Fhir.Utility;
using Hl7.Fhir.Model;
using MongoDB.Bson;

using Spark.Engine.Core;
using Spark.Mongo.Search.Common;
using Spark.Engine.Extensions;
using Spark.Search.Mongo;
using Spark.Engine.Search.Model;
using Spark.Engine.Search.Indexer;
using Spark.Engine.Search.Common;

namespace Spark.Mongo.Search.Indexer
{
    public class BsonIndexDocumentBuilder : FhirIndexDocumentBuilder<BsonDocument>
    {
       

        //public Document(MongoCollection<BsonDocument> collection, Definitions definitions)
        public BsonIndexDocumentBuilder(IKey key) : base(key)
        {
            //this.definitions = definitions;
        }

        public override BsonDocument ToDocument()
        {
            return Document;
        }
        
        public override void Write(String parameterName, FhirDateTime fhirDateTime)
        {
            BsonDocument value = new BsonDocument();
            value.Add(new BsonElement("start", BsonDateTime.Create(fhirDateTime.LowerBound())));
            value.Add(new BsonElement("end", BsonDateTime.Create(fhirDateTime.UpperBound())));
            Document.Write(parameterName, value);
        }

        public override void Write(Definition definition, string value)
        {
            if (definition.Argument != null)
                value = definition.Argument.GroomElement(value);
            if (definition.ParamType == Hl7.Fhir.Model.SearchParamType.Token && value != null)
            {
                var tokenValue = new BsonDocument
                {
                    {"code", value}
                };

                Document.Write(definition.ParamName, tokenValue);
            }
            else
            {
                Document.Write(definition.ParamName, value);
            }
        }

        // DSTU2: tags
        //public void Collect(Tag tag)
        //{
        //    string scheme = Assigned(tag.Scheme) ? tag.Scheme.ToString() : null;
        //    string term = tag.Term;
        //    string label = tag.Label;
        //    //string tagstring = glue("/", scheme, term);
        //    BsonDocument value = new BsonDocument()
        //        {
        //            { "scheme", scheme },
        //            { "term", term },
        //            { "label", label }
        //        };
        //    Write(InternalField.TAG, value);
        //}

        public override void Write(String parameterName, Quantity quantity)
        {
            BsonDocument block = quantity.ToBson();
            Document.Write(parameterName, block);
        }

        public override void Write(Definition definition, Coding coding)
        {
            BsonValue system = (coding.System != null) ? (BsonValue)coding.System : BsonNull.Value;
            BsonValue code = (coding.Code != null) ? (BsonValue)coding.Code : BsonNull.Value;

            var value = new BsonDocument
                {
                    { "system", system, system != null },
                    { "code", code },
                    { "display", coding.Display, coding.Display != null }
                };

            Document.Write(definition.ParamName, value);
        }

        public override void Write(Definition definition, Identifier identifier)
        {
            BsonValue system = (identifier.System != null) ? (BsonValue)identifier.System : BsonNull.Value;
            BsonValue code = (identifier.Value != null) ? (BsonValue)identifier.Value : BsonNull.Value;
            
            var value = new BsonDocument
                {
                    { "system", system },
                    { "code", code },
                    // eigenlijk moet het ook een Display bevatten (om dat search daarop kan zoeken bij een token)
                };
            Document.Write(definition.ParamName, value);
        }
       
        public override void Write(String parameterName, Period period)
        {
            BsonDocument value = new BsonDocument();
            if (period.StartElement != null)
                value.Add(new BsonElement("start", BsonDateTime.Create(period.StartElement.LowerBound())));
            if (period.EndElement != null)
                value.Add(new BsonElement("end", BsonDateTime.Create(period.EndElement.UpperBound())));
            Document.Write(parameterName, value);
        }

        public override void Write(string parameterName, string value)
        {
            Document.Write(parameterName, value);
        }

        public override void Write(string parameterName, int value)
        {
            Document.Write(parameterName, value);
        }
    }

}
