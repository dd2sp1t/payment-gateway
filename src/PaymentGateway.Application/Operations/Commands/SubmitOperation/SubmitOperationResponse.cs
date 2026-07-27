using System.Text.Json.Serialization;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.Commands.SubmitOperation;

public sealed record SubmitOperationResponse(
    string OperationId,
    OperationStatus Status,
    [property: JsonIgnore]
    bool NewlyScheduled);