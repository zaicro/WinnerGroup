namespace FunEvents.Infrastructure.Sql.Tables;

internal class TbReservation : AuditableTable
{
    public int Id { get; private set; }

    public string ReservationCode { get; set; } = null!;

    public int UserId { get; set; }

    public int EventId { get; set; }

    public int Quantity { get; set; }

    public int Channel { get; set; }

    public int Status { get; set; }

    public TbUser User { get; private set; } = null!;

    public TbEvent Event { get; private set; } = null!;
}
