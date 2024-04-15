using System.Collections;

namespace Reshaping.Extensions;

/// <summary>
/// 集成查询
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// 分组聚合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="table">数据库表</param>
    /// <returns></returns>
    public static IEnumerable<T> Unflatten<T>(this IQueryable table) => ((IEnumerable)Unflatten(string.Empty, typeof(IEnumerable<T>), table.Cast<object?>().AsEnumerable().Where(x => x != null).Select(x => x!.GetType().GetProperties().ToDictionary(y => y.Name, y => y.GetValue(x))).ToArray())!).Cast<T>();

    private static object? Unflatten(string key, Type type, IReadOnlyList<Dictionary<string, object?>> table)
    {
        object? value;
        if (type.IsPrimitive()) return table[0].TryGetValue(key, out value) ? value : null;
        if (type.IsAssignableTo(typeof(IEnumerable))) return table.GroupBy(x => new Dictionary<string, object?>(x.Where(y => y.Key.StartsWith(key) && type.GenericTypeArguments[0].GetPropertyByPath(y.Key[key.Length..]) is { } property && property.PropertyType.IsPrimitive())), new TableComparer()).Select(x => Unflatten(key, type.GenericTypeArguments[0], x.ToArray())).Cast(type.GenericTypeArguments[0]);
        if (table[0].All(x => !x.Key.StartsWith(key))) return null;
        value = Activator.CreateInstance(type);
        foreach (var x in type.GetProperties()) x.SetValue(value, Unflatten(key + x.Name, x.PropertyType, table));
        return value;
    }
}