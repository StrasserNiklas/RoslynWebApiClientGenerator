using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiGenerator.Extensions;

public static class EnumerableExtensions
{
    public static IDictionary<S, T> Merge<S, T>(this IEnumerable<IDictionary<S, T>> keyValuePairs) =>
        //keyValuePairs.Aggregate((d1, d2) => d1.Concat(d2))// .ToDictionary(k => k.Key, v => v.Value));
        keyValuePairs.SelectMany(x => x)
                     .GroupBy(d => d.Key)
                     .ToDictionary(x => x.Key, y => y.First().Value);

    public static string ConcatValues<S>(this IDictionary<S, string> keyValuePairs)
    {
        var stringBuilder = new StringBuilder();

        foreach (var value in keyValuePairs.Values)
        { 
            stringBuilder.AppendLine(value);
        }

        return stringBuilder.ToString();
    }

}
