namespace SrvSurvey.net
{
    internal class NetCache
    {
        private static Dictionary<string, object> cache = new();

        public static Func<Q, Task<T>> getCachingQuery<T, Q>(Func<Q, Task<T>> func) where Q : notnull // Not really
        {
            return async (q) =>
            {
                var key = $"{func.Target}/{func.Method}";
                if (!cache.ContainsKey(key)) cache[key] = new QueryCache<T, Q>();

                var queryCache = (QueryCache<T, Q>)cache[key];
                var rslt = await queryCache.query2(q, func);
                return rslt;
            };
        }

        public static async Task<T> query<T, Q>(Q query, Func<Task<T>> func) where Q : notnull
        {
            var key = $"{func.Target}/{func.Method}";
            if (!cache.ContainsKey(key!)) cache[key] = new QueryCache<T, Q>();

            var queryCache = (QueryCache<T, Q>)cache[key];
            var rslt = await queryCache.query(query, func);
            return rslt;
        }

        class QueryCache<T, Q> where Q : notnull
        {
            private Dictionary<Q, T> queries = new();

            public async Task<T> query2(Q query, Func<Q, Task<T>> func) // Not really
            {
                // use cached value if we have one
                var cacheValue = queries.GetValueOrDefault(query);
                if (cacheValue != null)
                    return cacheValue;

                // call to get a response, cache and return it
                var rslt = await func(query);
                queries[query] = rslt;
                return rslt;
            }

            public async Task<T> query(Q query, Func<Task<T>> func)
            {
                // use cached value if we have one
                var cacheValue = queries.GetValueOrDefault(query);
                if (cacheValue != null)
                    return cacheValue;

                // call to get a response, cache and return it
                var rslt = await func();
                queries[query] = rslt;
                return rslt;
            }
        }
    }
}
