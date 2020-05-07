using System;
using System.Collections.Generic;
using Spark.Engine.Core;
using Spark.Engine.Store.Interfaces;
using Spark.YesSql.Indexes;
using YesSql.Core.Services;
using YesSql.Core.Data;
using System.Linq;

namespace Spark.YesSql
{
    public class YesSqlFhirStore : IFhirStore
    {
        private readonly ISession _session;

        public YesSqlFhirStore(ISession session)
        {
            _session = session;
        }

        public void Add(Entry entry)
            => _session.Save(entry);
        

        public Entry Get(IKey key)
        {
            var query = _session.Query<Entry, EntryByKey>().Where(e=> e.Base == key.Base && e.ResourceId == key.ResourceId);

            if (key.HasVersionId())
            {
                query = query.Where(e => e.VersionId == key.VersionId);
            }

            return query.FirstOrDefault();
        }

        public IList<Entry> Get(IEnumerable<IKey> localIdentifiers)
        {
            return localIdentifiers.AsParallel().Select(key => Get(key)).ToList();
        }
    }
}
