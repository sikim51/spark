using System;
using Hl7.Fhir.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spark.Engine.Core;
using Spark.Engine.Search.Common;
using Spark.Engine.Search.Model;
using Spark.Lucene.Indexer;

namespace Spark.Lucene.Tests
{
    [TestClass]
    public class LuceneIndexDocumentBuilderTests
    {
        private Key _fakeKey = new Key("base", "type", "resourceid", "versionid");

        private Definition stringDefinition = new Definition() { ParamName = "fakeStringParam", ParamType = Hl7.Fhir.Model.SearchParamType.String };

        private Definition nullDefinition = new Definition() { ParamName = "fakeNullParam", ParamType = Hl7.Fhir.Model.SearchParamType.String };

        private Definition uriDefinition = new Definition() { ParamName = "fakeUriParam", ParamType = Hl7.Fhir.Model.SearchParamType.Uri };

        private Definition tokenDefinition = new Definition() { ParamName = "fakeTokenParam", ParamType = Hl7.Fhir.Model.SearchParamType.Token };

        private Definition argumentDefinition = new Definition() { ParamName = "fakeArgumentParam", ParamType = Hl7.Fhir.Model.SearchParamType.String, Argument = new Argument() };

        private Definition dateDefinition = new Definition() { ParamName = "fakeDateParam", ParamType = Hl7.Fhir.Model.SearchParamType.Date };

        private Definition periodDefinition = new Definition() { ParamName = "fakePeriodParam", ParamType = Hl7.Fhir.Model.SearchParamType.Special };

        private Definition codingDefinition = new Definition() { ParamName = "fakeDateParam", ParamType = Hl7.Fhir.Model.SearchParamType.Special };

        private Definition identifierDefinition = new Definition() { ParamName = "fakeIdentifierParam", ParamType = Hl7.Fhir.Model.SearchParamType.Special };

        private Definition resourceDefinition = new Definition() { ParamName = "fakeResourceParam", ParamType = Hl7.Fhir.Model.SearchParamType.Composite };


        [TestMethod]
        public void ShouldWriteString()
        {
            var builder = new LuceneIndexDocumentBuilder(_fakeKey);
            builder.InvokeWrite(stringDefinition, "stringValue");
            var document = builder.ToDocument();

            Assert.IsNotNull(document.GetField(stringDefinition.ParamName));
            Assert.AreEqual("stringValue", document.GetField(stringDefinition.ParamName).GetStringValue());
        }

        [TestMethod]
        public void ShouldWritePrimitiveString()
        {
            var builder = new LuceneIndexDocumentBuilder(_fakeKey);
            builder.InvokeWrite(stringDefinition, new FhirString("stringValue"));
            var document = builder.ToDocument();

            Assert.IsNotNull(document.GetField(stringDefinition.ParamName));
            Assert.AreEqual("stringValue", document.GetField(stringDefinition.ParamName).GetStringValue());
        }

        [TestMethod]
        public void ShouldWriteToken()
        {
            var builder = new LuceneIndexDocumentBuilder(_fakeKey);
            builder.InvokeWrite(tokenDefinition, "stringValue");
            var document = builder.ToDocument();

            Assert.IsNotNull(document.GetField($"{tokenDefinition.ParamName}_code"));
            Assert.AreEqual("stringValue", document.GetField($"{tokenDefinition.ParamName}_code").GetStringValue());
        }

        [TestMethod]
        public void ShouldWriteArgument()
        {
            var builder = new LuceneIndexDocumentBuilder(_fakeKey);
            builder.InvokeWrite(argumentDefinition, "stringValue");
            var document = builder.ToDocument();

            Assert.IsNotNull(document.GetField(argumentDefinition.ParamName));
            Assert.AreEqual("stringValue", document.GetField(argumentDefinition.ParamName).GetStringValue());
        }

        [TestMethod]
        public void ShouldWriteUri()
        {
            var builder = new LuceneIndexDocumentBuilder(_fakeKey);
            builder.InvokeWrite(uriDefinition, new FhirUri("http://localhost"));
            var document = builder.ToDocument();

            Assert.IsNotNull(document.GetField(uriDefinition.ParamName));
            Assert.AreEqual("http://localhost", document.GetField(uriDefinition.ParamName).GetStringValue());
        }


        [TestMethod]
        public void ShouldWriteNullUri()
        {
            var builder = new LuceneIndexDocumentBuilder(_fakeKey);
            builder.InvokeWrite(uriDefinition, (FhirUri)null);
            var document = builder.ToDocument();

            Assert.IsNotNull(document.GetField(uriDefinition.ParamName));
            Assert.AreEqual(string.Empty, document.GetField(uriDefinition.ParamName).GetStringValue());
        }

        [TestMethod]
        public void ShouldWriteDate()
        {
            var builder = new LuceneIndexDocumentBuilder(_fakeKey);
            var dateTime = FhirDateTime.Now();
            builder.InvokeWrite(dateDefinition, dateTime);
            var document = builder.ToDocument();

            Assert.IsNotNull(document.GetField($"{dateDefinition.ParamName}_start"));
            Assert.IsNotNull(document.GetField($"{dateDefinition.ParamName}_end"));
        }

        [TestMethod]
        public void ShouldWriteCodeableConcept()
        {
            var builder = new LuceneIndexDocumentBuilder(_fakeKey);

            var concept = new CodeableConcept("system", "code", "display", "text");
            builder.InvokeWrite(codingDefinition, concept);
            var document = builder.ToDocument();

            Assert.IsNotNull(document.GetField($"{codingDefinition.ParamName}_text"));
            Assert.AreEqual("text", document.GetField($"{codingDefinition.ParamName}_text").GetStringValue());
            Assert.IsNotNull(document.GetField($"{codingDefinition.ParamName}_system"));
            Assert.AreEqual("system", document.GetField($"{codingDefinition.ParamName}_system").GetStringValue());
            Assert.IsNotNull(document.GetField($"{codingDefinition.ParamName}_code"));
            Assert.AreEqual("code", document.GetField($"{codingDefinition.ParamName}_code").GetStringValue());
            Assert.IsNotNull(document.GetField($"{codingDefinition.ParamName}_display"));
            Assert.AreEqual("display", document.GetField($"{codingDefinition.ParamName}_display").GetStringValue());
        }

        [TestMethod]
        public void ShouldWriteMetadataAtLevel0()
        {
            var builder = new LuceneIndexDocumentBuilder(_fakeKey);
            builder.WriteMetaData(_fakeKey, 0, new Person() { Meta = new Meta() { LastUpdated = DateTimeOffset.Now } });
            var document = builder.ToDocument();

            Assert.IsNotNull(document.GetField(IndexFieldNames.ID));
            Assert.AreEqual(builder.RootId, document.GetField(IndexFieldNames.ID).GetStringValue());
            Assert.IsNotNull(document.GetField(IndexFieldNames.SELFLINK));
            Assert.IsNotNull(document.GetField(IndexFieldNames.JUSTID));
            Assert.IsNotNull(document.GetField($"{IndexFieldNames.LASTUPDATED}_start"));
            Assert.IsNotNull(document.GetField($"{IndexFieldNames.LASTUPDATED}_end"));
        }

        [TestMethod]
        public void ShouldWriteMetadataAtLevel1()
        {
            var builder = new LuceneIndexDocumentBuilder(_fakeKey);
            var person = new Person() { Id = "id", Meta = new Meta() { LastUpdated = DateTimeOffset.Now } };
            builder.WriteMetaData(_fakeKey, 1, person);
            var document = builder.ToDocument();

            Assert.IsNotNull(document.GetField(IndexFieldNames.ID));
            Assert.AreEqual($"{builder.RootId}#{person.Id}", document.GetField(IndexFieldNames.ID).GetStringValue());
            Assert.IsNull(document.GetField(IndexFieldNames.SELFLINK));
            Assert.IsNull(document.GetField(IndexFieldNames.JUSTID));
            Assert.IsNull(document.GetField($"{IndexFieldNames.LASTUPDATED}_start"));
            Assert.IsNull(document.GetField($"{IndexFieldNames.LASTUPDATED}_end"));
        }

        [TestMethod]
        public void ShouldWriteResource()
        {
            var builder = new LuceneIndexDocumentBuilder(_fakeKey);
            var person = new Person() { Id = "id", Meta = new Meta() { LastUpdated = DateTimeOffset.Now } };
            builder.InvokeWrite(resourceDefinition, person);
            var document = builder.ToDocument();

            Assert.IsNotNull(document.GetField(resourceDefinition.ParamName));
            Assert.AreEqual(nameof(Person), document.GetField(resourceDefinition.ParamName).GetStringValue());
        }

        [TestMethod]
        public void ShouldWritePeriod()
        {
            var builder = new LuceneIndexDocumentBuilder(_fakeKey);
            var startDateTime = FhirDateTime.Now();
            var endDateTime = FhirDateTime.Now();
            builder.InvokeWrite(periodDefinition, new Period(startDateTime, endDateTime));
            var document = builder.ToDocument();

            Assert.IsNotNull(document.GetField($"{periodDefinition.ParamName}_start"));
            Assert.IsNotNull(document.GetField($"{periodDefinition.ParamName}_end"));
        }

        [TestMethod]
        public void ShouldWriteIdentifier()
        {
            var builder = new LuceneIndexDocumentBuilder(_fakeKey);
            builder.InvokeWrite(periodDefinition, new Identifier("system", "value"));
            var document = builder.ToDocument();

            Assert.IsNotNull(document.GetField($"{periodDefinition.ParamName}_system"));
            Assert.IsNotNull(document.GetField($"{periodDefinition.ParamName}_code"));
        }

        [TestMethod]
        public void ShouldWriteNullObject()
        {
            var builder = new LuceneIndexDocumentBuilder(_fakeKey);
            builder.InvokeWrite(nullDefinition, (object)null);
            var document = builder.ToDocument();

            Assert.IsNotNull(document.GetField(nullDefinition.ParamName));
            Assert.AreEqual(string.Empty, document.GetField(nullDefinition.ParamName).GetStringValue());
        }
    }
}
