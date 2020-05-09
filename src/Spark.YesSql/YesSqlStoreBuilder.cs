using Spark.Engine.Store.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Spark.YesSql
{
    public class YesSqlStorageBuilder : IStorageBuilder
    {
        public T GetStore<T>()
        {
            return default(T);
        }
    }
}
