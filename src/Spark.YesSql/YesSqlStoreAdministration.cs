using System;
using Spark.Engine.Interfaces;
using Spark.YesSql.Indexes;
using YesSql;
using YesSql.Sql;

namespace Spark.YesSql
{
    public class YesSqlStoreAdministration : IFhirStoreAdministration
    {
        private readonly IStore _store;

        public YesSqlStoreAdministration(IStore store)
        {
            _store = store;
        }

        public void Clean()
        {
            EnsureIndices();
        }

        private void EnsureIndices()
        {
            using (var connection = _store.Configuration.ConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel))
                {
                    new SchemaBuilder(_store.Configuration, transaction)
                        .CreateMapIndexTable(nameof(EntryByKey), table => table
                            .Column<string>(nameof(EntryByKey.Base))
                            .Column<string>(nameof(EntryByKey.ResourceId))
                            .Column<string>(nameof(EntryByKey.TypeName))
                            .Column<string>(nameof(EntryByKey.VersionId))
                        );

                    transaction.Commit();
                }
            }
        }
    }
}
