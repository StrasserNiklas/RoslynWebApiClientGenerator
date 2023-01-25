using System.Collections.Generic;
using System.Linq;

namespace ApiGenerator.Extensions;

public static class EnumerableExtensions
{
    public static IDictionary<S, T> Merge<S, T>(this IEnumerable<IDictionary<S, T>> keyValuePairs ) =>
        keyValuePairs.Aggregate((d1, d2) => d1.Concat(d2).ToDictionary(k => k.Key, v => v.Value));
}
