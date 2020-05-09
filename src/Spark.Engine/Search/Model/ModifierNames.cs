using System;
namespace Spark.Engine.Search.Model
{
    public static class ModifierNames
    {
        [Obsolete]
        public const string
            BEFORE = "before",
            AFTER = "after",
            Separator = ":";

        public const string
            EXACT = "exact",
            CONTAINS = "contains",
            PARTIAL = "partial",
            TEXT = "text",
            CODE = "code",
            ANYNAMESPACE = "anyns",
            MISSING = "missing",
            BELOW = "below",
            ABOVE = "above",
            NOT = "not",
            NONE = "";
    }
}
