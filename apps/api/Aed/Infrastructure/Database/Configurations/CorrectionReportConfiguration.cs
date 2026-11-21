using Domain.Modules.AedDevice.Model;
using Domain.Modules.CorrectionReport.Constants;
using Domain.Modules.CorrectionReport.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

/// <summary>
/// Mapping of <see cref="CorrectionReport"/>.
/// </summary>
internal sealed class CorrectionReportConfiguration : IEntityTypeConfiguration<CorrectionReport>
{
    public void Configure(EntityTypeBuilder<CorrectionReport> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Message).HasMaxLength(CorrectionReportConstant.MessageMaxLength).IsRequired();
        builder.Property(x => x.ReporterContact).HasMaxLength(CorrectionReportConstant.ReporterContactMaxLength);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ResolutionNote).HasMaxLength(CorrectionReportConstant.ResolutionNoteMaxLength);
        builder.HasOne<AedDevice>().WithMany().HasForeignKey(x => x.AedDeviceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.Status);
    }
}
