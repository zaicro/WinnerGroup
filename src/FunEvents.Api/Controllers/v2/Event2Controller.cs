using FunEvents.Application.Features.Event.Model.Commands;
using FunEvents.Application.Features.Event.Model.Queries;
using FunEvents.Application.Features.Event.Services;

namespace FunEvents.Api.Controllers.v2;

[ApiVersion("2.0")]
[Route("api/v2/[controller]")]
[ApiController]
public class Event2Controller(ICreateEventService createEventService, IUpdateEventService updateEventService, IGetEventService getEventService) : ControllerBase
{
    private readonly ICreateEventService _createEventService = createEventService ?? throw new ArgumentNullException(nameof(createEventService));
    private readonly IUpdateEventService _updateEventService = updateEventService ?? throw new ArgumentNullException(nameof(updateEventService));
    private readonly IGetEventService _getEventService = getEventService ?? throw new ArgumentNullException(nameof(getEventService));

    [Authorize(AuthenticationSchemes = "ApiKey")]
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateEventCommand command, CancellationToken cancellationToken)
    {
        var result = await _createEventService.CreateEventAsync(command, cancellationToken);

        return Ok(result);
    }

    [Authorize(AuthenticationSchemes = "ApiKey")]
    [HttpPut("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateEventCommand command, CancellationToken cancellationToken)
    {
        var result = await _updateEventService.UpdateEventAsync(command, cancellationToken);

        return Ok(result);
    }

    [HttpGet("getAll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await _getEventService.GetAllAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("getByCode")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByCodeAsync([FromQuery] GetByCodeQuery query, CancellationToken cancellationToken)
    {
        var result = await _getEventService.GetByCodeAsync(query, cancellationToken);

        return Ok(result);
    }
}