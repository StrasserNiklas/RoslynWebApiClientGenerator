using System;
using System.Collections.Generic;
using System.Text;

namespace ApiGenerator.Extensions;

public static class ObjectExtensions
{
    public static bool IsNullableObject<T>(this T obj)
    {
        return default(T) == null;
    }

    public static bool IsNullableValueType<T>(this T obj)
    {
        return default(T) == null && typeof(T).BaseType != null && "ValueType".Equals(typeof(T).BaseType.Name);
    }
}
