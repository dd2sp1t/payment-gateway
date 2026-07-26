using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.Operations.CreateOperation;
using PaymentGateway.Application.Operations.SubmitOperation;

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

    [HttpPost("{operationId}/submit")]
    [ProducesResponseType<SubmitOperationResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<SubmitOperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubmitOperationResponse>> Submit(
        [FromRoute] string operationId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new SubmitOperationCommand(operationId), cancellationToken);

        return response.NewlyScheduled
            ? Accepted(response)
            : Ok(response);
    }
}