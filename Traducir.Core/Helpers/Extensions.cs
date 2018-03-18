using System.Collections.Generic;

namespace Traducir.Core.Helpers
{
    public static class Extensions
    {
        public static HashSet<T> ToHashSet<T>(
            this IEnumerable<T> source,
            IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool HasValue(this string str)
        {
            return !str.IsNullOrEmpty();
        }
    }
}