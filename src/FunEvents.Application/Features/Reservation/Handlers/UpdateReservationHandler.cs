using FunEvents.Application.Features.Reservation.Models.Commands;
using FunEvents.Application.Features.Reservation.Models.DTOs;
using FunEvents.Application.Features.Reservation.Services;

namespace FunEvents.Application.Features.Reservation.Handlers;

internal sealed class UpdateReservationHandler(IUpdateReservationService service) : IRequestHandler<UpdateReservationCommand, Response<ReservationDto>>
{
    private readonly IUpdateReservationService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task<Response<ReservationDto>> Handle(UpdateReservationCommand request, CancellationToken cancellationToken)
    {
        var dto = await _service.UpdateReservationAsync(request, cancellationToken).ConfigureAwait(false);
        return Response<ReservationDto>.Success(dto);
    }
}