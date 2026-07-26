using System.Text.Json.Serialization;

namespace PaymentGateway.Application.Abstractions.PaymentProvider.Models;

public sealed record SubmitPaymentRequest(
    [property: JsonPropertyName("operationId")]
    string OperationId,

    [property: JsonPropertyName("amount")]
    string Amount,

    [property: JsonPropertyName("currency")]
    string Currency);