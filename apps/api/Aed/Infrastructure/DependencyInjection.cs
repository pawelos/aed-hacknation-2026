using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Configurations;
using Application.Constants;
using Infrastructure.Authentication;
using Infrastructure.Database;
using Infrastructure.Identity;
using Infrastructure.Repositories;
using Infrastructure.Seeders;
using Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

namespace Infrastructure;

/// <summary>
/// Registers the infrastructure services: PostgreSQL with PostGIS, Identity and JWT bearer authentication.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Name of the connection string of the AED database.
    /// </summary>
    public const string DatabaseConnectionName = "Database";

    /// <summary>
    /// Registers the infrastructure services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddHttpContextAccessor();
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));
        services.Configure<OpenDataOptions>(configuration.GetSection(OpenDataOptions.SectionName));
        services.AddSingleton<Application.Abstractions.Import.IImportDownloader, Import.ImportDownloader>();

        AddDatabase(services, configuration);
        AddAuthentication(services, configuration);

        return services;
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString(DatabaseConnectionName)
            ?? throw new InvalidOperationException($"ConnectionStrings:{DatabaseConnectionName} is not set.");

        services.AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql(
                connectionString,
                npgsql => npgsql
                    .UseNetTopologySuite()
                    .MigrationsHistoryTable(HistoryRepository.DefaultTableName, BaseSchemeConstant.Base))
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
        services.AddScoped(typeof(IRepositoryAsync<,>), typeof(RepositoryAsync<,>));
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 10;
                options.User.RequireUniqueEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        JwtOptions jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        if (jwt.Secret.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Secret must have at least 32 characters.");
        }

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserContext, UserContext>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep claim types as written (sub, role, owner_id).
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    // JwtTokenService signs with HS256 only; any other "alg" header is rejected.
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    // Always on: an empty Issuer/Audience must fail closed, not disable the check.
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = JwtRegisteredClaimNames.Email,
                    RoleClaimType = AedClaimTypes.Role
                };
            });

        services.AddAuthorization();
    }
}
