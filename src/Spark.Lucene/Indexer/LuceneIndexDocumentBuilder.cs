using System;
using Hl7.Fhir.Model;
using Lucene.Net.Documents;
using Spark.Engine.Core;
using Spark.Engine.Extensions;
using Spark.Engine.Search.Common;
using Spark.Engine.Search.Indexer;

namespace Spark.Lucene.Indexer
{
    public class LuceneIndexDocumentBuilder : FhirIndexDocumentBuilder<Document>
    {
        public LuceneIndexDocumentBuilder(IKey key): base(key)
        {
        }

        public override Document ToDocument()
        {
            return Document;
        }

        public override void Write(string parameterName, FhirDateTime fhirDateTime)
        {
            Document.Add(new StoredField($"{parameterName}_start", DateTools.DateToString(fhirDateTime.LowerBound().UtcDateTime, DateTools.Resolution.SECOND)));
            Document.Add(new StoredField($"{parameterName}_end", DateTools.DateToString(fhirDateTime.UpperBound().UtcDateTime, DateTools.Resolution.SECOND)));
        }

        public override void Write(Definition definition, string value)
        {
            if (definition.Argument != null)
                value = definition.Argument.GroomElement(value);

            if (definition.ParamType == Hl7.Fhir.Model.SearchParamType.Token && value != null)
            {
               Document.Add(new StoredField($"{definition.ParamName}_code", value ?? string.Empty));
            }
            else
            {
               Document.Add(new StoredField(definition.ParamName, value ?? String.Empty));
            }
        }

        public override void Write(string paramName, Quantity quantity)
        {
            throw new NotImplementedException();
        }

        public override void Write(Definition definition, Coding coding)
        {
            if (coding.System != null)
            {
                Document.Add(new StoredField($"{definition.ParamName}_system", coding.System));
            }

            Document.Add(new StoredField($"{definition.ParamName}_code", coding.Code));

            if (coding.Display != null)
            {
                Document.Add(new StoredField($"{definition.ParamName}_display", coding.Display));
            }
        }

        public override void Write(Definition definition, Identifier identifier)
        {
            Document.Add(new StoredField($"{definition.ParamName}_system", identifier.System));
            Document.Add(new StoredField($"{definition.ParamName}_code", identifier.Value));
        }
   
        public override void Write(string parameterName, Period period)
        {
            Document.Add(new StoredField($"{parameterName}_start", DateTools.DateToString(period.StartElement.LowerBound().UtcDateTime, DateTools.Resolution.SECOND)));
            Document.Add(new StoredField($"{parameterName}_end", DateTools.DateToString(period.EndElement.UpperBound().UtcDateTime, DateTools.Resolution.SECOND)));
        }

        public override void Write(string paramName, string value)
        {
            Document.Add(new StoredField(paramName, value));
        }

        public override void Write(string paramName, int value)
        {
            Document.Add(new StoredField(paramName, value));
        }
    }
}
