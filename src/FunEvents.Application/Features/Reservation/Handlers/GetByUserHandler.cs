using FunEvents.Application.Features.Reservation.Models.DTOs;
using FunEvents.Application.Features.Reservation.Models.Queries;
using FunEvents.Application.Features.Reservation.Services;

namespace FunEvents.Application.Features.Reservation.Handlers;

internal sealed class GetByUserHandler(IGetReservationService service) : IRequestHandler<GetByUserQuery, Response<List<ReservationDto>>>
{
    private readonly IGetReservationService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task<Response<List<ReservationDto>>> Handle(GetByUserQuery request, CancellationToken cancellationToken)
    {
        var dto = await _service.GetByUserAsync(request, cancellationToken).ConfigureAwait(false);
        return Response<List<ReservationDto>>.Success(dto);
    }
}