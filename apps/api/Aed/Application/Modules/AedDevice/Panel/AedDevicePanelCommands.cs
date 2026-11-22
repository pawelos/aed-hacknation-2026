using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Modules.AedImport;
using Domain.Modules.AedDevice.Model;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using AedDeviceEntity = Domain.Modules.AedDevice.Model.AedDevice;

namespace Application.Modules.AedDevice.Panel;

/// <summary>
/// Adds an AED record to a source of the user's organisation.
/// </summary>
public sealed class CreateAedDeviceCommand : AedDeviceInput, ICommand<Guid>
{
    /// <summary>
    /// Source of the record.
    /// </summary>
    public Guid DataSourceId { get; set; }
}

/// <summary>
/// Changes an AED record. Saving counts as the owner's confirmation: the record is verified now and its
/// verification flag is cleared.
/// </summary>
public sealed class UpdateAedDeviceCommand : AedDeviceInput, ICommand
{
    /// <summary>
    /// Record to change.
    /// </summary>
    public Guid Id { get; set; }
}

/// <summary>
/// Removes a record from the data set (soft delete; it disappears from the map and the files).
/// </summary>
public sealed record DeleteAedDeviceCommand(Guid Id) : ICommand;

/// <summary>
/// The owner confirms the record is up to date: verified now, flag cleared, "to verify" becomes "working".
/// </summary>
public sealed record VerifyAedDeviceCommand(Guid Id) : ICommand;

internal sealed class CreateAedDeviceCommandValidator : AbstractValidator<CreateAedDeviceCommand>
{
    public CreateAedDeviceCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.DataSourceId).NotEmpty();
        AedDeviceInputRules.Add(this, timeProvider);
    }
}

internal sealed class UpdateAedDeviceCommandValidator : AbstractValidator<UpdateAedDeviceCommand>
{
    public UpdateAedDeviceCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Id).NotEmpty();
        AedDeviceInputRules.Add(this, timeProvider);
    }
}

internal sealed class CreateAedDeviceCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<CreateAedDeviceCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(CreateAedDeviceCommand command, CancellationToken cancellationToken)
    {
        var source = await DataSourceAccess.GetAsync(dbContext, userContext, command.DataSourceId, cancellationToken);
        if (source.IsFailure)
        {
            return Result.Failure<Guid>(source.Error);
        }

        string sourceRecordId = command.SourceRecordId.Trim();
        if (sourceRecordId.Length > 0 && await dbContext.AedDevices.AnyAsync(
                d => d.DataSourceId == command.DataSourceId && !d.IsDeleted && d.SourceRecordId == sourceRecordId, cancellationToken))
        {
            return Result.Failure<Guid>(AedDeviceErrors.SourceRecordIdTaken(sourceRecordId));
        }

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        var device = new AedDeviceEntity
        {
            Id = Guid.NewGuid(),
            DataSourceId = source.Value.Id,
            OwnerId = source.Value.OwnerId,
            CreatedAt = now,
            CreatedBy = userContext.UserId,
            LastVerifiedAt = now
        };
        AedDeviceInputRules.Apply(command, device);
        dbContext.AedDevices.Add(device);
        await dbContext.SaveChangesAsync(cancellationToken);
        return device.Id;
    }
}

internal sealed class UpdateAedDeviceCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<UpdateAedDeviceCommand>
{
    public async ValueTask<Result> Handle(UpdateAedDeviceCommand command, CancellationToken cancellationToken)
    {
        AedDeviceEntity? device = await AedDeviceAccess.Visible(dbContext, userContext)
            .FirstOrDefaultAsync(d => d.Id == command.Id, cancellationToken);
        if (device is null)
        {
            return Result.Failure(AedDeviceErrors.NotFound(command.Id));
        }

        string sourceRecordId = command.SourceRecordId.Trim();
        if (sourceRecordId.Length > 0 && await dbContext.AedDevices.AnyAsync(
                d => d.DataSourceId == device.DataSourceId && d.Id != device.Id && !d.IsDeleted && d.SourceRecordId == sourceRecordId,
                cancellationToken))
        {
            return Result.Failure(AedDeviceErrors.SourceRecordIdTaken(sourceRecordId));
        }

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        AedDeviceInputRules.Apply(command, device);
        device.LastVerifiedAt = now;
        device.NeedsVerification = false;
        device.VerificationReason = string.Empty;
        device.UpdatedAt = now;
        device.UpdatedBy = userContext.UserId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteAedDeviceCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<DeleteAedDeviceCommand>
{
    public async ValueTask<Result> Handle(DeleteAedDeviceCommand command, CancellationToken cancellationToken)
    {
        AedDeviceEntity? device = await AedDeviceAccess.Visible(dbContext, userContext)
            .FirstOrDefaultAsync(d => d.Id == command.Id, cancellationToken);
        if (device is null)
        {
            return Result.Failure(AedDeviceErrors.NotFound(command.Id));
        }

        device.SoftDelete(userContext.UserId, timeProvider.GetUtcNow().UtcDateTime);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class VerifyAedDeviceCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<VerifyAedDeviceCommand>
{
    public async ValueTask<Result> Handle(VerifyAedDeviceCommand command, CancellationToken cancellationToken)
    {
        AedDeviceEntity? device = await AedDeviceAccess.Visible(dbContext, userContext)
            .FirstOrDefaultAsync(d => d.Id == command.Id, cancellationToken);
        if (device is null)
        {
            return Result.Failure(AedDeviceErrors.NotFound(command.Id));
        }

        device.ConfirmUpToDate(userContext.UserId, timeProvider.GetUtcNow().UtcDateTime);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
