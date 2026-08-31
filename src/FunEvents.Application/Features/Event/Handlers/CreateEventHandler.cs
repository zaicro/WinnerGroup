using FunEvents.Application.Features.Event.Model.Commands;
using FunEvents.Application.Features.Event.Model.DTOs;
using FunEvents.Application.Features.Event.Services;

namespace FunEvents.Application.Features.Event.Handlers;

internal sealed class CreateEventHandler(ICreateEventService service) : IRequestHandler<CreateEventCommand, Response<EventDto>>
{
    private readonly ICreateEventService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task<Response<EventDto>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var dto = await _service.CreateEventAsync(request, cancellationToken).ConfigureAwait(false);
        return Response<EventDto>.Success(dto);
    }
}