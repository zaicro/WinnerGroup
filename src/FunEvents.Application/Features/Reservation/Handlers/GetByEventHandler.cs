using FunEvents.Application.Features.Reservation.Models.DTOs;
using FunEvents.Application.Features.Reservation.Models.Queries;
using FunEvents.Application.Features.Reservation.Services;

namespace FunEvents.Application.Features.Reservation.Handlers;

internal sealed class GetByEventHandler(IGetReservationService service) : IRequestHandler<GetByEventQuery, Response<List<ReservationDto>>>
{
    private readonly IGetReservationService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task<Response<List<ReservationDto>>> Handle(GetByEventQuery request, CancellationToken cancellationToken)
    {
        var dto = await _service.GetByEventAsync(request, cancellationToken).ConfigureAwait(false);
        return Response<List<ReservationDto>>.Success(dto);
    }
}