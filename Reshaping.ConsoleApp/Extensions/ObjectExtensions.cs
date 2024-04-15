using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Reshaping.ConsoleApp.Extensions;

internal static class ObjectExtensions
{
    public static void Dump<T, TResult>(this T value, Func<T, TResult> selector) => selector(value).Dump();

    public static void Dump<T>(this T value) => Console.WriteLine(JsonSerializer.Serialize(value, new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    }));
}