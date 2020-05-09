using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Spark.Engine.Core;
using Spark.Engine.Extensions;
using Spark.Engine.Search.Common;
using Spark.Engine.Store.Interfaces;

namespace Spark.Engine.Search.Indexer
{
    public abstract class FhirIndexer<T>
        where T : IIndexStore
    {
        protected T Store;
        protected Definitions Definitions;

        public FhirIndexer(IIndexStore store, Definitions definitions)
        {
            this.Store = (T)store;
            this.Definitions = definitions;
        }

        public void Process(Entry entry)
        {
            if (entry.HasResource())
            {
                put(entry);
            }
            else
            {
                if (entry.IsDeleted())
                {
                    Store.Delete(entry);
                }
                else throw new Exception("Entry is neither resource nor deleted");
            }
        }

        public void Process(IEnumerable<Entry> entries)
        {
            foreach (Entry entry in entries)
            {
                Process(entry);
            }
        }

        protected abstract void put(IKey key, int level, DomainResource resource);

        private void put(IKey key, int level, IEnumerable<Resource> resources)
        {
            if (resources == null) return;
            foreach (var resource in resources)
            {
                if (resource is DomainResource)
                    put(key, level, resource as DomainResource);
            }
        }

        private void put(IKey key, int level, Resource resource)
        {
            if (resource is DomainResource)
            {
                DomainResource d = resource as DomainResource;
                put(key, level, d);
                put(key, level + 1, d.Contained);
            }

        }

        private void put(Entry entry)
        {
            put(entry.Key, 0, entry.Resource);
        }
    }
}
