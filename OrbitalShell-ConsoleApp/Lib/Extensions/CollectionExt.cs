using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitalShell.Lib.Extensions
{
    public static class CollectionExt
    {
        public static bool TryGet<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate,
            out TSource firstOrDefault)
        {
            firstOrDefault = source.FirstOrDefault(x => predicate(x));
            return firstOrDefault != null;
        }

        public static bool TryGet<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, int, bool> predicate,
            out TSource firstOrDefault)
        {
            firstOrDefault = source.Where((x, i) => predicate(x, i)).FirstOrDefault();
            return firstOrDefault != null;
        }
    }
}
