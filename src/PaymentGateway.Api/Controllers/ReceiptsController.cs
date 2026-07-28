using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.Operations.Commands.ProcessReceipt;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("receipts")]
public sealed class ReceiptsController : ControllerBase
{
    private readonly ISender _sender;

    public ReceiptsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Process([FromBody] ProcessReceiptCommand command, CancellationToken _)
    {
        await _sender.Send(command, CancellationToken.None);

        return NoContent();
    }
}