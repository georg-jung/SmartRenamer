using System;
using System.Collections.Generic;
using System.Text;

namespace SmartRenamer.Core
{
    internal static class IEumerableExtensions
    {
        /// <summary>
        /// Enumerates over tuples of pairs of the original sequence. I.e. { 1, 2, 3 } becomes { (1, 2), (2, 3) }. Note that { 1 } becomes { }.
        /// </summary>
        public static IEnumerable<(T, T)> Pairwise<T>(this IEnumerable<T> source)
        {
            using var it = source.GetEnumerator();

            if (!it.MoveNext())
                yield break;

            var previous = it.Current;

            while (it.MoveNext())
                yield return (previous, previous = it.Current);
        }
    }
}
