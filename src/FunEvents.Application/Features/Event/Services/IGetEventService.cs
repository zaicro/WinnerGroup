using FunEvents.Application.Features.Event.Model.DTOs;
using FunEvents.Application.Features.Event.Model.Queries;

namespace FunEvents.Application.Features.Event.Services;

public interface IGetEventService
{
    Task<List<EventDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<EventDto> GetByCodeAsync(GetByCodeQuery request, CancellationToken cancellationToken);
}
