using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GronkBowl.Infrastructure;

/// <summary>
/// Generic helpers for mapping a complex CLR value (a list, a dictionary, a record) to a
/// single jsonb column - the pragmatic v1 choice over fully normalizing every nested structure
/// (a Roster's depth chart, a Play's assignments, a Match's event log) into its own table. The
/// value comparer matters: without it, EF's change tracking won't notice an in-place
/// `list.Add(...)`, only a full reassignment of the property.
/// </summary>
public static class JsonValueConverters
{
    public static ValueConverter<T, string> For<T>() => new(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<T>(v, (JsonSerializerOptions?)null)!);

    public static ValueComparer<T> ComparerFor<T>() => new(
        (a, b) => Serialize(a) == Serialize(b),
        v => Serialize(v).GetHashCode(),
        v => JsonSerializer.Deserialize<T>(Serialize(v), (JsonSerializerOptions?)null)!);

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null);
}
