using FunEvents.Application.Contracts;
using FunEvents.Application.Features.Reservation.Models.Commands;
using FunEvents.Application.Features.Reservation.Models.DTOs;
using FunEvents.Application.Features.Reservation.Services;

namespace FunEvents.Application.Features.Reservation.Handlers;

internal sealed class CreateReservationHandler(ICreateReservationService service, IIdempotencyKeyProvider idempotencyKeyProvider) : IRequestHandler<CreateReservationCommand, Response<ReservationDto>>
{
    private readonly ICreateReservationService _service = service ?? throw new ArgumentNullException(nameof(service));
    private readonly IIdempotencyKeyProvider _idempotencyKeyProvider = idempotencyKeyProvider ?? throw new ArgumentNullException(nameof(idempotencyKeyProvider));

    public async Task<Response<ReservationDto>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var idempotencyKey = _idempotencyKeyProvider.Get();

        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("Idempotency-Key header is required.");

        var dto = await _service.CreateReservationAsync(request, idempotencyKey, cancellationToken).ConfigureAwait(false);
        return Response<ReservationDto>.Success(dto);
    }
}