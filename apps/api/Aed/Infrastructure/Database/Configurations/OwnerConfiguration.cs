using Domain.Modules.Owner.Constants;
using Domain.Modules.Owner.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

/// <summary>
/// Mapping of <see cref="Owner"/>.
/// </summary>
internal sealed class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(OwnerConstant.NameMaxLength).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Email).HasMaxLength(OwnerConstant.EmailMaxLength);
        builder.Property(x => x.Phone).HasMaxLength(OwnerConstant.PhoneMaxLength);
        builder.Property(x => x.Website).HasMaxLength(OwnerConstant.WebsiteMaxLength);
        builder.Property(x => x.Regon).HasMaxLength(OwnerConstant.RegonMaxLength);
        builder.HasIndex(x => x.Name);
    }
}
