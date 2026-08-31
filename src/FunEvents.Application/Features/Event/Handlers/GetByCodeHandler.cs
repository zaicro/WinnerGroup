using FunEvents.Application.Features.Event.Model.DTOs;
using FunEvents.Application.Features.Event.Model.Queries;
using FunEvents.Application.Features.Event.Services;

namespace FunEvents.Application.Features.Event.Handlers;

internal sealed class GetByCodeHandler(IGetEventService service) : IRequestHandler<GetByCodeQuery, Response<EventDto>>
{
    private readonly IGetEventService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task<Response<EventDto>> Handle(GetByCodeQuery request, CancellationToken cancellationToken)
    {
        var dto = await _service.GetByCodeAsync(request, cancellationToken).ConfigureAwait(false);
        return Response<EventDto>.Success(dto);
    }
}