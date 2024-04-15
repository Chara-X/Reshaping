using System.Reflection;

namespace Reshaping.Extensions;

internal static class TypeExtensions
{
    public static PropertyInfo? GetPropertyByPath(this Type type, string path)
    {
        var property = type.GetProperty(path);
        if (property != null) return property;
        property = type.GetProperties().Where(x => path.StartsWith(x.Name)).MaxBy(x => x.Name.Length);
        return property == null ? null : GetPropertyByPath(property.PropertyType, path[property.Name.Length..]);
    }

    public static bool IsPrimitive(this Type type) => type.IsPrimitive || type == typeof(string);
}