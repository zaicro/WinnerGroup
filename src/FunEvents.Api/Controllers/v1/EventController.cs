using FunEvents.Application.Features.Event.Model.Commands;
using FunEvents.Application.Features.Event.Model.Queries;

namespace FunEvents.Api.Controllers.v1;

[ApiVersion("1.0")]
public class EventController(IMediator mediator) : BaseApiController(mediator)
{
    [Authorize(AuthenticationSchemes = "ApiKey")]
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateEventCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [Authorize(AuthenticationSchemes = "ApiKey")]
    [HttpPut("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateEventCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [Authorize(AuthenticationSchemes = "ApiKey")]
    [HttpGet("getAll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync([FromQuery] GetAllQuery command)
    {
        return Ok(await Mediator.Send(command));
    }

    [Authorize(AuthenticationSchemes = "ApiKey")]
    [HttpGet("getByCode")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsync([FromBody] GetByCodeQuery command)
    {
        return Ok(await Mediator.Send(command));
    }
}
