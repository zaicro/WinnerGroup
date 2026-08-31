using FunEvents.Application.Features.Event.Model.Commands;
using FunEvents.Application.Features.Event.Model.DTOs;

namespace FunEvents.Application.Features.Event.Services;

public interface IUpdateEventService
{
    Task<EventDto> UpdateEventAsync(UpdateEventCommand request, CancellationToken cancellationToken);
}
