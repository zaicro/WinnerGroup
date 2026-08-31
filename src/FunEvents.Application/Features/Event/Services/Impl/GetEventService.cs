using FunEvents.Application.Features.Event.Model.DTOs;
using FunEvents.Application.Features.Event.Model.Queries;

namespace FunEvents.Application.Features.Event.Services.Impl;

internal sealed class GetEventService(IEventRepository eventRepository, ILogger logger) : IGetEventService
{
    private readonly IEventRepository _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<EventDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();

        try
        {
            _logger.Info(method!, "Start");

            var result = await _eventRepository.GetAllAsync(cancellationToken);

            _logger.Info(method!, "End");

            return
            [
                .. result.Select(x => new EventDto
                {
                    Code = x.Code,
                    Name = x.Name,
                    EventDate = x.EventDate,
                    Capacity = x.Capacity,
                    Status = new OptionDto
                    {
                        Code = (int)x.Status,
                        Name = x.Status.ToString()
                    }
                })
            ];
        }
        catch (Exception ex)
        {
            _logger.Error(method!, "Error", ex);

            throw;
        }
    }

    public async Task<EventDto> GetByCodeAsync(GetByCodeQuery request, CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();

        try
        {
            _logger.Info(method!, "Start");

            var result = await _eventRepository.GetByCodeAsync(request.Code, cancellationToken);

            _logger.Info(method!, "End");

            return result is null
                ? throw new KeyNotFoundException("Event was not found.")
                : new EventDto
                {
                    Code = result.Code,
                    Name = result.Name,
                    EventDate = result.EventDate,
                    Capacity = result.Capacity,
                    Status = new OptionDto
                    {
                        Code = (int)result.Status,
                        Name = result.Status.ToString()
                    }
                };
        }
        catch (Exception ex)
        {
            _logger.Error(method!, "Error", ex);

            throw;
        }
    }
}
