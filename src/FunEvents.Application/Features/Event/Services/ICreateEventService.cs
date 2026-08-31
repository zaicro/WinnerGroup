using FunEvents.Application.Features.Event.Model.Commands;
using FunEvents.Application.Features.Event.Model.DTOs;

namespace FunEvents.Application.Features.Event.Services;

public interface ICreateEventService
{
    Task<EventDto> CreateEventAsync(CreateEventCommand request, CancellationToken cancellationToken);
}
