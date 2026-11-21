using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Database;

/// <summary>
/// Mapping conventions shared by all entities derived from <see cref="SharedKernel.BaseEntity"/>.
/// </summary>
internal static class BaseEntityConfiguration
{
    /// <summary>
    /// Maps the soft delete audit columns of all base entities.
    /// </summary>
    /// <param name="modelBuilder">Model builder.</param>
    public static void ApplyConventions(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            modelBuilder.Entity(entityType.ClrType, builder =>
            {
                builder.Property(nameof(BaseEntity.DeletedAt));
                builder.Property(nameof(BaseEntity.DeletedBy));
            });
        }
    }
}
