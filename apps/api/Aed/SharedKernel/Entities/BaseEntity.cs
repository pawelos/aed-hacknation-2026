namespace SharedKernel;

/// <summary>
/// Base class for entities with a Guid identifier, audit fields and soft delete support.
/// </summary>
public abstract class BaseEntity : Entity, IEntity<Guid>
{
    /// <summary>
    /// Unique identifier of the entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Indicates whether the entity is soft deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// UTC date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Identifier of the user who created the entity.
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// UTC date and time of the last update.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Identifier of the user who last updated the entity.
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// UTC date and time when the entity was soft deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Identifier of the user who soft deleted the entity.
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Marks the entity as deleted without removing it from the database.
    /// </summary>
    /// <param name="deletedBy">Identifier of the user performing the deletion.</param>
    /// <param name="utcNow">Current UTC date and time.</param>
    public void SoftDelete(Guid? deletedBy, DateTime utcNow)
    {
        IsDeleted = true;
        DeletedAt = utcNow;
        DeletedBy = deletedBy;
        UpdatedAt = utcNow;
        UpdatedBy = deletedBy;
    }

    /// <summary>
    /// Restores a soft deleted entity and clears its deletion data.
    /// </summary>
    /// <param name="updatedBy">Identifier of the user performing the restore.</param>
    /// <param name="utcNow">Current UTC date and time.</param>
    public void Restore(Guid? updatedBy, DateTime utcNow)
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        UpdatedAt = utcNow;
        UpdatedBy = updatedBy;
    }
}
