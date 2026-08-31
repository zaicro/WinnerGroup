using FunEvents.Application.Features.Event.Model.DTOs;
using FunEvents.Application.Features.Event.Model.Queries;
using FunEvents.Application.Features.Event.Services;

namespace FunEvents.Application.Features.Event.Handlers;

internal sealed class GetAllHandler(IGetEventService service) : IRequestHandler<GetAllQuery, Response<List<EventDto>>>
{
    private readonly IGetEventService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task<Response<List<EventDto>>> Handle(GetAllQuery request, CancellationToken cancellationToken)
    {
        var dto = await _service.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return Response<List<EventDto>>.Success(dto);
    }
}