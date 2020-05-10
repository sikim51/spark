﻿using System;
using Hl7.Fhir.Model;
using Spark.Core;
using Spark.YesSql.Models;
using YesSql;

namespace Spark.YesSql
{
    public class YesSqlIdGenerator : IGenerator
    {
        private readonly ISession _session;

        public YesSqlIdGenerator(ISession session)
        {
            _session = session;
        }

        public string NextResourceId(Resource resource)
         => this.Next(resource.TypeName);

        public string NextVersionId(string resourceIdentifier)
        {
            throw new NotImplementedException();
        }

        public string NextVersionId(string resourceType, string resourceIdentifier)
        {
            string name = resourceType + "_history_" + resourceIdentifier;
            return this.Next(name);
        }

        private string Next(string typeName)
        {
            return null;   
        }
    }
}