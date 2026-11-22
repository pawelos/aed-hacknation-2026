namespace SharedKernel;

/// <summary>
/// Route segments shared by the API controllers.
/// </summary>
public static class BaseConstant
{
    /// <summary>
    /// Name of the identifier route parameter.
    /// </summary>
    public const string Id = "Id";
    /// <summary>
    /// Route segment with the identifier of a record.
    /// </summary>
    public const string ById = "{id:guid}";

    /// <summary>
    /// Route segment for creating a record.
    /// </summary>
    public const string Create = "create";
    /// <summary>
    /// Route segment for updating a record.
    /// </summary>
    public const string Update = "update";
    /// <summary>
    /// Route segment for moving a record up, down, first or last in a user-ordered list.
    /// </summary>
    public const string Move = "move";
    /// <summary>
    /// Route segment for deleting a record.
    /// </summary>
    public const string Delete = "delete";

    /// <summary>
    /// Route segment for active records.
    /// </summary>
    public const string Active = "active";
    /// <summary>
    /// Route segment for inactive records.
    /// </summary>
    public const string Inactive = "inactive";

    /// <summary>
    /// Route segment for the list of records.
    /// </summary>
    public const string List = "list";
    /// <summary>
    /// Route segment for viewing a record.
    /// </summary>
    public const string View = "view";

    /// <summary>
    /// Route segment for completing a record.
    /// </summary>
    public const string Complete = "complete";

    /// <summary>
    /// Route segment for reopening a completed record.
    /// </summary>
    public const string Reopen = "reopen";
    /// <summary>
    /// Route segment for roles.
    /// </summary>
    public const string Roles = "roles";

    /// <summary>
    /// Route segment for soft deleted records.
    /// </summary>
    public const string Trash = "trash";
    /// <summary>
    /// Route segment for restoring a soft deleted record.
    /// </summary>
    public const string Restore = "{id:guid}/restore";
    /// <summary>
    /// Route segment for deleting multiple records.
    /// </summary>
    public const string BatchDelete = "batch-delete";
    /// <summary>
    /// Route segment for restoring multiple records.
    /// </summary>
    public const string BatchRestore = "batch-restore";
    /// <summary>
    /// Route segment for confirming multiple records as up to date.
    /// </summary>
    public const string BatchVerify = "batch-verify";
    /// <summary>
    /// Route segment for permanently deleting records from the trash.
    /// </summary>
    public const string Purge = "purge";
    /// <summary>
    /// Route segment for a single page of records.
    /// </summary>
    public const string Paged = "paged";
}
