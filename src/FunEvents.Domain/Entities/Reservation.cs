namespace FunEvents.Domain.Entities;

public class Reservation
{
    public Reservation()
    {
    }

    public Reservation(
        string reservationCode,
        int eventId,
        int userId,
        int quantity,
        ReservationChannel channel,
        ReservationStatus status)
    {
        if (quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.",
                nameof(quantity));

        ReservationCode = reservationCode;
        EventId = eventId;
        UserId = userId;
        Quantity = quantity;
        Channel = channel;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public string ReservationCode { get; set; } = null!;

    public int EventId { get; set; }

    public int UserId { get; set; }

    public int Quantity { get; set; }

    public ReservationChannel Channel { get; set; }

    public ReservationStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}