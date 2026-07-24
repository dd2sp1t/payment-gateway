using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.Operations.CreateOperation;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("operations")]
public sealed class OperationsController : ControllerBase
{
    private readonly ISender _sender;

    public OperationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType<CreateOperationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateOperationResponse>> Create(
        [FromBody] CreateOperationCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(command, cancellationToken);

        return Created(string.Empty, response);
    }
}