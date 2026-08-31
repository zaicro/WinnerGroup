using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FunEvents.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class BaseApiController : ControllerBase
{
    //private IMediator _mediator;

    //protected IMediator Mediator => _mediator ?? (_mediator = base.HttpContext.RequestServices.GetService<IMediator>());

    protected IMediator Mediator { get; }

    protected BaseApiController(IMediator mediator)
    {
        Mediator = mediator;
    }
}