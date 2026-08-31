using FunEvents.Application.Features.Reservation.Models.Commands;
using FunEvents.Application.Features.Reservation.Models.DTOs;
using FunEvents.Application.Features.Reservation.Services;

namespace FunEvents.Application.Features.Reservation.Handlers;

internal sealed class CreateReservationHandler(ICreateReservationService service) : IRequestHandler<CreateReservationCommand, Response<ReservationDto>>
{
    private readonly ICreateReservationService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task<Response<ReservationDto>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var dto = await _service.CreateReservationAsync(request, cancellationToken).ConfigureAwait(false);
        return Response<ReservationDto>.Success(dto);
    }
}