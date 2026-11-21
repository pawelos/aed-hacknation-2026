using Domain.Modules.DataSource.Constants;
using Domain.Modules.DataSource.Model;
using Domain.Modules.Owner.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

/// <summary>
/// Mapping of <see cref="DataSource"/>.
/// </summary>
internal sealed class DataSourceConfiguration : IEntityTypeConfiguration<DataSource>
{
    public void Configure(EntityTypeBuilder<DataSource> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(DataSourceConstant.NameMaxLength).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(DataSourceConstant.UrlMaxLength);
        builder.Property(x => x.License).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ReferenceScope).HasMaxLength(DataSourceConstant.ReferenceScopeMaxLength);
        builder.HasOne<Owner>().WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
    }
}
