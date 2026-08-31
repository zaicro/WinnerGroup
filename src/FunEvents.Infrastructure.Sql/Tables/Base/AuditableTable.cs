namespace FunEvents.Infrastructure.Sql.Tables.Base;

internal abstract class AuditableTable
{
    protected AuditableTable()
    {
        IsActive = true;
        IsDeleted = false;
        CreatedBy = "Unauthorized";
        CreatedAt = DateTime.UtcNow;
        ModifiedBy = "Unauthorized";
        ModifiedAt = DateTime.UtcNow;
        Remarks = "Remarks by Default";
    }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string ModifiedBy { get; set; } = null!;

    public DateTime ModifiedAt { get; set; }

    public string Remarks { get; set; } = null!;
}