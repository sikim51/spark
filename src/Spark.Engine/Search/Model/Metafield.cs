using System;
namespace Spark.Engine.Search.Model
{
    public static class MetaField
    {
        public const string
            COUNT = "_count",
            INCLUDE = "_include",
            LIMIT = "_limit"; // Limit is geen onderdeel vd. standaard

        public static string[] All = { COUNT, INCLUDE, LIMIT };
    }
}
