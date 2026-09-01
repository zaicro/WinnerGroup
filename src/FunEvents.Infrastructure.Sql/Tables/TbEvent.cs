namespace FunEvents.Infrastructure.Sql.Tables;

internal class TbEvent : AuditableTable
{
    public int Id { get; private set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public int Capacity { get; set; }

    public int AvailableCapacity { get; set; }

    public int Status { get; set; }

    public ICollection<TbReservation> Reservations { get; private set; } = [];
}
