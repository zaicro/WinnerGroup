using FunEvents.Application.Features.Event.Model.Commands;
using FunEvents.Application.Features.Event.Model.DTOs;
using FunEvents.Domain.Repositories;

namespace FunEvents.Application.Features.Event.Services.Impl;

public sealed class CreateEventService(IUnitOfWork unitOfWork, IEventRepository eventRepository, ILogger logger) : ICreateEventService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IEventRepository _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<EventDto> CreateEventAsync(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var method = MethodBase.GetCurrentMethod();

        try
        {
            _logger.Info(method!, "Start");

            var newRecord = new Domain.Entities.Event(
                0,
                request.Code,
                request.Name,
                request.EventDate,
                request.Capacity,
                request.Capacity,
                Domain.Enums.EventStatus.Draft
                );

            await EnsureEventDoesNotExistAsync(newRecord, cancellationToken);

            await _eventRepository.AddAsync(newRecord, cancellationToken);
            
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.Info(method!, "End");

            return new EventDto
            {
                Code = newRecord.Code,
                Name = newRecord.Name,
                EventDate = newRecord.EventDate,
                Capacity = newRecord.Capacity,
                Status = new OptionDto
                {
                    Code = (int)newRecord.Status,
                    Name = newRecord.Status.ToString()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.Error(method!, "Error", ex);
            await _unitOfWork.RollBackAsync(cancellationToken);
            throw;
        }
    }

    private async Task EnsureEventDoesNotExistAsync(Domain.Entities.Event @event, CancellationToken cancellationToken)
    {
        var usernameExists = await _eventRepository.ExistsAsync(@event.Code, cancellationToken);

        if (usernameExists) throw new ArgumentException("Event is already registered.", nameof(@event.Code));
    }
}
