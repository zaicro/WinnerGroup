using FunEvents.Application.Features.Event.Model.Commands;
using FunEvents.Application.Features.Event.Model.DTOs;
using FunEvents.Application.Features.Event.Services;

namespace FunEvents.Application.Features.Event.Handlers;

internal sealed class UpdateEventHandler(IUpdateEventService service) : IRequestHandler<UpdateEventCommand, Response<EventDto>>
{
    private readonly IUpdateEventService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task<Response<EventDto>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var dto = await _service.UpdateEventAsync(request, cancellationToken).ConfigureAwait(false);
        return Response<EventDto>.Success(dto);
    }
}