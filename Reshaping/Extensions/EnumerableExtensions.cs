using System.Collections;

namespace Reshaping.Extensions;

/// <summary>
/// 集成查询
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// 多表连接
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">数据序列</param>
    /// <returns></returns>
    public static IQueryable Flatten<T>(this IEnumerable<T> source) => Flatten(string.Empty, source).AsQueryable();

    private static IEnumerable<Dictionary<string, object?>> Flatten(string key, object? value)
    {
        if (value == null) return Enumerable.Empty<Dictionary<string, object?>>().DefaultIfEmpty(new Dictionary<string, object?>());
        if (value.GetType().IsPrimitive()) return new[] { new Dictionary<string, object?> { { key, value } } };
        if (value is IEnumerable enumerable) return enumerable.Cast<object?>().SelectMany(x => Flatten(key, x));
        return value.GetType().GetProperties().Select(property => Flatten(key + property.Name, property.GetValue(value))).Aggregate(Enumerable.Empty<Dictionary<string, object?>>().DefaultIfEmpty(new Dictionary<string, object?>()), (current, inner) => current.Join(inner, _ => true, _ => true, (x, y) => x.Concat(y).ToDictionary(z => z.Key, z => z.Value)));
    }

    internal static IEnumerable Cast(this IEnumerable source, Type type)
    {
        var genericList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type))!;
        foreach (var x in source)
            genericList.Add(x);
        return genericList;
    }
}