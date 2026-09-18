using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GronkBowl.Infrastructure;

public static class ModelBuilderExtensions
{
    /// <summary>Maps this property to a single jsonb column, with a value comparer so EF
    /// notices in-place mutations of the underlying list/dictionary, not just reassignment.</summary>
    public static PropertyBuilder<T> AsJson<T>(this PropertyBuilder<T> builder)
    {
        builder.HasConversion(JsonValueConverters.For<T>());
        builder.HasColumnType("jsonb");
        builder.Metadata.SetValueComparer(JsonValueConverters.ComparerFor<T>());
        return builder;
    }
}
