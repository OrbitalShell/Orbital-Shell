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
            var lst = source.Where(x => predicate(x));
            firstOrDefault = source.FirstOrDefault();
            return source.Any();
        }

        public static bool TryGet<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, int, bool> predicate,
            out TSource firstOrDefault)
        {
            var lst = source.Where((x, i) => predicate(x, i));
            firstOrDefault = source.FirstOrDefault();
            return source.Any();
        }
    }
}
