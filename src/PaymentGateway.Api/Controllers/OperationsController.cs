using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.Abstractions.Persistence.ReadModels;
using PaymentGateway.Application.Operations.CreateOperation;
using PaymentGateway.Application.Operations.Models;
using PaymentGateway.Application.Operations.Queries.GetOperation;
using PaymentGateway.Application.Operations.Queries.GetOperationEvents;
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
    [ProducesResponseType<OperationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OperationResponse>> Create(
        [FromBody] CreateOperationCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(Get),
            new { operationId = response.OperationId },
            response);
    }

    [HttpPost("{operationId}/submit")]
    [ProducesResponseType<SubmitOperationResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<SubmitOperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    [HttpGet("{operationId}")]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OperationResponse>> Get(
        [FromRoute] string operationId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetOperationQuery(operationId), cancellationToken);

        return Ok(response);
    }

    [HttpGet("{operationId}/events")]
    [ProducesResponseType<IReadOnlyList<OperationEventReadModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<OperationEventReadModel>>> GetOperationEvents(
        [FromRoute] string operationId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetOperationEventsQuery(operationId), cancellationToken);

        return Ok(response);
    }
}