using FunEvents.Application.Features.User.Model.Commands;
using FunEvents.Application.Features.User.Model.Queries;

namespace FunEvents.Api.Controllers.v1;

[ApiVersion("1.0")]
public class UserController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUserCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateUserCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpGet("getAll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAsync([FromQuery] GetAllQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("getByEmail")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByEmailAsync([FromQuery] GetByEmailQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("getByUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUserAsync([FromQuery] GetByUserQuery query)
    {
        return Ok(await Mediator.Send(query));
    }
}
