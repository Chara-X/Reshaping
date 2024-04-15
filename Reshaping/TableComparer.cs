namespace Reshaping;

internal class TableComparer : IEqualityComparer<Dictionary<string, object?>>
{
    public bool Equals(Dictionary<string, object?>? x, Dictionary<string, object?>? y) => x!.SequenceEqual(y!);
    public int GetHashCode(Dictionary<string, object?> obj) => obj.Aggregate(0, (current, x) => current ^ x.GetHashCode());
}