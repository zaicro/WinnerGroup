namespace FunEvents.ConsoleApp.Reservation;

internal sealed record CreateReservationRequest(
    string Code,
    string EventCode,
    string UserName,
    int Quantity,
    int Channel);

internal sealed record UpdateReservationRequest(
    string Code,
    int Quantity,
    ReservationStatusRequest Status);

internal sealed record ReservationStatusRequest(
    int Code,
    string Name);