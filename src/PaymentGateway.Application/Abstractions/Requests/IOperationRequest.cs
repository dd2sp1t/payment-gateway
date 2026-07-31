namespace PaymentGateway.Application.Abstractions.Requests;

public interface IOperationRequest
{
    string OperationId { get; }
}