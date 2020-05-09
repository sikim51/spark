using System;
using Spark.Engine.Core;
using YesSql.Indexes;

namespace Spark.YesSql.Indexes
{
    public class EntryIndexProvider : IndexProvider<Entry>
    {

        public override void Describe(DescribeContext<Entry> context)
        {
            context.For<EntryByKey>()
                .Map(entry =>
                   new EntryByKey
                   {
                       Base = entry.Key.Base,
                       TypeName = entry.Key.TypeName,
                       ResourceId = entry.Key.ResourceId,
                       VersionId = entry.Key.VersionId
                   }
                );
        }
    }
}
