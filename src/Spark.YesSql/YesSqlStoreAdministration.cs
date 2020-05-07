using System;
using Spark.Engine.Interfaces;
using YesSql;

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

                using (var transaction = connection.BeginTransaction(store.Configuration.IsolationLevel))
                {
                    new SchemaBuilder(store.Configuration, transaction)
                        .CreateReduceIndexTable(nameof(ArticleByWord), table => table
                            .Column<int>("Count")
                            .Column<string>("Word")
                        );

                    transaction.Commit();
                }
            }
        }
    }
}
