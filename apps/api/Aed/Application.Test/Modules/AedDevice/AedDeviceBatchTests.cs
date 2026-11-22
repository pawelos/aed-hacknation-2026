using Application.Modules.AedDevice.Panel;
using Domain.Modules.AedDevice.Model;
using FluentValidation.Results;
using AedDeviceEntity = Domain.Modules.AedDevice.Model.AedDevice;

namespace Application.Test.Modules.AedDevice;

/// <summary>
/// Batch commands of the panel list: the id list rules and the owner's confirmation of a record.
/// </summary>
public sealed class AedDeviceBatchTests
{
    private static ValidationResult Validate(List<Guid> ids) =>
        new BatchDeleteAedDevicesCommandValidator().Validate(new BatchDeleteAedDevicesCommand { Ids = ids });

    [Test]
    public void One_to_a_hundred_ids_are_valid()
    {
        Assert.That(Validate([Guid.NewGuid()]).IsValid, Is.True);
        Assert.That(Validate(Enumerable.Range(0, AedDeviceBatchRules.MaxIds).Select(_ => Guid.NewGuid()).ToList()).IsValid, Is.True);
    }

    [Test]
    public void An_empty_list_is_rejected()
    {
        ValidationResult result = Validate([]);

        Assert.That(result.Errors.Select(e => e.ErrorCode), Does.Contain("AedDevice.BatchEmpty"));
    }

    [Test]
    public void More_than_a_hundred_ids_are_rejected()
    {
        ValidationResult result = new BatchVerifyAedDevicesCommandValidator().Validate(new BatchVerifyAedDevicesCommand
        {
            Ids = Enumerable.Range(0, AedDeviceBatchRules.MaxIds + 1).Select(_ => Guid.NewGuid()).ToList()
        });

        Assert.That(result.Errors.Select(e => e.ErrorCode), Does.Contain("AedDevice.BatchTooLarge"));
    }

    [Test]
    public void An_empty_id_is_rejected()
    {
        Assert.That(Validate([Guid.Empty]).IsValid, Is.False);
    }

    [Test]
    public void Confirming_clears_the_flag_and_turns_to_verify_into_working()
    {
        var now = new DateTime(2026, 11, 22, 10, 0, 0, DateTimeKind.Utc);
        Guid userId = Guid.NewGuid();
        var device = new AedDeviceEntity { NeedsVerification = true, VerificationReason = "Report", Status = AedStatus.ToVerify };

        device.ConfirmUpToDate(userId, now);

        Assert.Multiple(() =>
        {
            Assert.That(device.LastVerifiedAt, Is.EqualTo(now));
            Assert.That(device.NeedsVerification, Is.False);
            Assert.That(device.VerificationReason, Is.Empty);
            Assert.That(device.Status, Is.EqualTo(AedStatus.Operational));
            Assert.That(device.UpdatedBy, Is.EqualTo(userId));
        });
    }

    [Test]
    public void Confirming_keeps_a_withdrawn_device_withdrawn()
    {
        var device = new AedDeviceEntity { Status = AedStatus.Withdrawn };

        device.ConfirmUpToDate(Guid.NewGuid(), DateTime.UtcNow);

        Assert.That(device.Status, Is.EqualTo(AedStatus.Withdrawn));
    }
}
