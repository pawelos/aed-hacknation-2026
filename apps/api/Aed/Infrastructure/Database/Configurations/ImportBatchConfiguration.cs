using Domain.Modules.DataSource.Model;
using Domain.Modules.ImportBatch.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

/// <summary>
/// Mapping of <see cref="ImportBatch"/>.
/// </summary>
internal sealed class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Format).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.FileName).HasMaxLength(1000);
        builder.Property(x => x.ReportJson).HasColumnType("jsonb");
        builder.HasOne<DataSource>().WithMany().HasForeignKey(x => x.DataSourceId).OnDelete(DeleteBehavior.Restrict);
    }
}
