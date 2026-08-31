namespace FunEvents.Infrastructure.Sql.Tables;

internal class TbUser : AuditableTable
{
    public int Id { get; private set; }

    public string Username { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public ICollection<TbReservation> Reservations { get; private set; } = [];
}
