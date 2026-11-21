using Application.Abstractions.Data;
using Domain.Modules.Owner.Model;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using AedDeviceEntity = Domain.Modules.AedDevice.Model.AedDevice;
using CorrectionReportEntity = Domain.Modules.CorrectionReport.Model.CorrectionReport;
using DataSourceEntity = Domain.Modules.DataSource.Model.DataSource;
using ImportBatchEntity = Domain.Modules.ImportBatch.Model.ImportBatch;

namespace Infrastructure.Database;

/// <summary>
/// EF Core context of the AED database (PostgreSQL with PostGIS), including the ASP.NET Core Identity tables.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    /// <inheritdoc />
    public DbSet<Owner> Owners => Set<Owner>();

    /// <inheritdoc />
    public DbSet<DataSourceEntity> DataSources => Set<DataSourceEntity>();

    /// <inheritdoc />
    public DbSet<AedDeviceEntity> AedDevices => Set<AedDeviceEntity>();

    /// <inheritdoc />
    public DbSet<CorrectionReportEntity> CorrectionReports => Set<CorrectionReportEntity>();

    /// <inheritdoc />
    public DbSet<ImportBatchEntity> ImportBatches => Set<ImportBatchEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(BaseSchemeConstant.Base);
        // Geography points and the GiST index of AedDevice.Location; the init script also creates it locally.
        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        BaseEntityConfiguration.ApplyConventions(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable(IdentityTableNames.Users, BaseSchemeConstant.Base);
            entity.Property(u => u.DisplayName).HasMaxLength(200);
            entity.HasIndex(u => u.OwnerId);
        });

        modelBuilder.Entity<IdentityRole<Guid>>().ToTable(IdentityTableNames.Roles, BaseSchemeConstant.Base);
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable(IdentityTableNames.UserRoles, BaseSchemeConstant.Base);
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable(IdentityTableNames.UserClaims, BaseSchemeConstant.Base);
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable(IdentityTableNames.UserLogins, BaseSchemeConstant.Base);
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable(IdentityTableNames.UserTokens, BaseSchemeConstant.Base);
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable(IdentityTableNames.RoleClaims, BaseSchemeConstant.Base);
    }
}
