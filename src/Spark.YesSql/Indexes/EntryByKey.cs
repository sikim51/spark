using System;
using YesSql.Indexes;

namespace Spark.YesSql.Indexes
{
    public class EntryByKey : MapIndex
    {
        public string Base { get; set; }
        public string TypeName { get; set; }
        public string ResourceId { get; set; }
        public string VersionId { get; set; }
    }
}
