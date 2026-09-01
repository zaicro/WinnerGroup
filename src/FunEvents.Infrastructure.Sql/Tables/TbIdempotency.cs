namespace FunEvents.Infrastructure.Sql.Tables;

internal class TbIdempotency : AuditableTable
{
    public long Id { get; set; }

    public string Key { get; set; } = null!;

    public string TableName { get; set; } = null!;

    public long TableKeyValue { get; set; }
}
