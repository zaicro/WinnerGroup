using FunEvents.Application.Features.Reservation.Models.DTOs;
using FunEvents.Application.Features.Reservation.Models.Queries;
using FunEvents.Application.Features.Reservation.Services;

namespace FunEvents.Application.Features.Reservation.Handlers;

internal sealed class GetByCodeHandler(IGetReservationService service) : IRequestHandler<GetByCodeQuery, Response<ReservationDto>>
{
    private readonly IGetReservationService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task<Response<ReservationDto>> Handle(GetByCodeQuery request, CancellationToken cancellationToken)
    {
        var dto = await _service.GetByCodeAsync(request, cancellationToken).ConfigureAwait(false);
        return Response<ReservationDto>.Success(dto);
    }
}