using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Common.Localization;
using Domain.Modules.AedDevice.Model;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using AedDeviceEntity = Domain.Modules.AedDevice.Model.AedDevice;

namespace Application.Modules.AedDevice.Panel;

/// <summary>
/// Removes the chosen records from the data set in one go (soft delete, the selection toolbar of the panel list).
/// All or nothing: when any record is missing or belongs to another owner, nothing changes. Returns how many were
/// removed.
/// </summary>
public sealed class BatchDeleteAedDevicesCommand : ICommand<int>
{
    /// <summary>
    /// Records to remove (1–100, repeated ids count once).
    /// </summary>
    public List<Guid> Ids { get; set; } = [];
}

/// <summary>
/// The owner confirms the chosen records are up to date, as <see cref="VerifyAedDeviceCommand"/> for each of them.
/// All or nothing, like <see cref="BatchDeleteAedDevicesCommand"/>. Returns how many were confirmed.
/// </summary>
public sealed class BatchVerifyAedDevicesCommand : ICommand<int>
{
    /// <summary>
    /// Records to confirm (1–100, repeated ids count once).
    /// </summary>
    public List<Guid> Ids { get; set; } = [];
}

/// <summary>
/// Shared rules of the batch commands: at least one and at most <see cref="MaxIds"/> ids, none empty.
/// </summary>
internal static class AedDeviceBatchRules
{
    /// <summary>
    /// The most records in one request (four pages of the panel list).
    /// </summary>
    public const int MaxIds = 100;

    /// <summary>
    /// The list of ids: not empty, at most <see cref="MaxIds"/>.
    /// </summary>
    public static IRuleBuilderOptions<T, List<Guid>> BatchIds<T>(this IRuleBuilder<T, List<Guid>> rule) =>
        rule
            .NotEmpty().WithErrorCode("AedDevice.BatchEmpty")
            .WithMessage(_ => Lang.T("Choose at least one record.", "Zaznacz co najmniej jeden rekord."))
            .Must(list => list.Count <= MaxIds).WithErrorCode("AedDevice.BatchTooLarge")
            .WithMessage(_ => Lang.T($"At most {MaxIds} records at a time.", $"Najwyżej {MaxIds} rekordów naraz."));

    /// <summary>
    /// The owner's confirmation of one record: verified now, flag and reason cleared, "to verify" becomes
    /// "working" (also used by <see cref="VerifyAedDeviceCommand"/>).
    /// </summary>
    public static void ConfirmUpToDate(this AedDeviceEntity device, Guid? userId, DateTime now)
    {
        device.LastVerifiedAt = now;
        device.NeedsVerification = false;
        device.VerificationReason = string.Empty;
        if (device.Status == AedStatus.ToVerify)
        {
            device.Status = AedStatus.Operational;
        }

        device.UpdatedAt = now;
        device.UpdatedBy = userId;
    }

    /// <summary>
    /// The visible records with the given ids, or the error when any of them is missing.
    /// </summary>
    public static async Task<Result<List<AedDeviceEntity>>> LoadAllAsync(
        IApplicationDbContext dbContext,
        IUserContext userContext,
        List<Guid> requested,
        CancellationToken cancellationToken)
    {
        List<Guid> ids = requested.Distinct().ToList();
        List<AedDeviceEntity> devices = await AedDeviceAccess.Visible(dbContext, userContext)
            .Where(d => ids.Contains(d.Id))
            .ToListAsync(cancellationToken);
        return devices.Count == ids.Count
            ? devices
            : Result.Failure<List<AedDeviceEntity>>(AedDeviceErrors.BatchNotFound(ids.Count - devices.Count, ids.Count));
    }
}

internal sealed class BatchDeleteAedDevicesCommandValidator : AbstractValidator<BatchDeleteAedDevicesCommand>
{
    public BatchDeleteAedDevicesCommandValidator()
    {
        RuleFor(x => x.Ids).BatchIds();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}

internal sealed class BatchVerifyAedDevicesCommandValidator : AbstractValidator<BatchVerifyAedDevicesCommand>
{
    public BatchVerifyAedDevicesCommandValidator()
    {
        RuleFor(x => x.Ids).BatchIds();
        RuleForEach(x => x.Ids).NotEmpty();
    }
}

internal sealed class BatchDeleteAedDevicesCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<BatchDeleteAedDevicesCommand, int>
{
    public async ValueTask<Result<int>> Handle(BatchDeleteAedDevicesCommand command, CancellationToken cancellationToken)
    {
        Result<List<AedDeviceEntity>> devices = await AedDeviceBatchRules.LoadAllAsync(dbContext, userContext, command.Ids, cancellationToken);
        if (devices.IsFailure)
        {
            return Result.Failure<int>(devices.Error);
        }

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (AedDeviceEntity device in devices.Value)
        {
            device.SoftDelete(userContext.UserId, now);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return devices.Value.Count;
    }
}

internal sealed class BatchVerifyAedDevicesCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<BatchVerifyAedDevicesCommand, int>
{
    public async ValueTask<Result<int>> Handle(BatchVerifyAedDevicesCommand command, CancellationToken cancellationToken)
    {
        Result<List<AedDeviceEntity>> devices = await AedDeviceBatchRules.LoadAllAsync(dbContext, userContext, command.Ids, cancellationToken);
        if (devices.IsFailure)
        {
            return Result.Failure<int>(devices.Error);
        }

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (AedDeviceEntity device in devices.Value)
        {
            device.ConfirmUpToDate(userContext.UserId, now);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return devices.Value.Count;
    }
}
