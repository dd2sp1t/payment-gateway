using PaymentGateway.Domain.Exceptions;

namespace PaymentGateway.Domain.Operations;

public sealed class Operation
{
    #region Properties

    public OperationId OperationId { get; }

    public Guid? ProviderPaymentId { get; private set; }

    public decimal Amount { get; }

    public string Currency { get; }

    public string Description { get; }

    public OperationStatus Status { get; private set; }

    #endregion

    #region Constructors

    private Operation(
        OperationId operationId,
        Guid? providerPaymentId,
        decimal amount,
        string currency,
        string description,
        OperationStatus status)
    {
        OperationId = operationId;
        ProviderPaymentId = providerPaymentId;
        Amount = amount;
        Currency = currency;
        Description = description;
        Status = status;
    }

    #endregion

    #region Factory methods

    public static Operation Create(
        OperationId operationId,
        decimal amount,
        string currency,
        string description)
    {
        if (amount <= 0)
        {
            throw new DomainException("Operation amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Currency is required.");
        }

        if (string.Equals(currency, Currencies.Rub, StringComparison.Ordinal) == false)
        {
            throw new DomainException($"Currency '{currency}' is not supported.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Description is required.");
        }

        return new Operation(
            operationId,
            providerPaymentId: null,
            amount,
            currency,
            description,
            OperationStatus.Created);
    }

    internal static Operation Restore(
        OperationId operationId,
        Guid? providerPaymentId,
        decimal amount,
        string currency,
        string description,
        OperationStatus status)
    {
        return new Operation(
            operationId,
            providerPaymentId,
            amount,
            currency,
            description,
            status);
    }

    #endregion

    #region Public methods

    public void StartProcessing()
    {
        if (Status != OperationStatus.Created)
        {
            throw new DomainException(
                $"Operation '{OperationId}' cannot be moved from '{Status}' to '{OperationStatus.Processing}'.");
        }

        Status = OperationStatus.Processing;
    }

    public void Complete(Guid providerPaymentId)
    {
        if (Status != OperationStatus.Processing)
        {
            throw new DomainException(
                $"Operation '{OperationId}' cannot be completed from '{Status}'.");
        }

        SetProviderPaymentId(providerPaymentId);

        Status = OperationStatus.Completed;
    }

    public void Reject(Guid providerPaymentId)
    {
        if (Status != OperationStatus.Processing)
        {
            throw new DomainException(
                $"Operation '{OperationId}' cannot be rejected from '{Status}'.");
        }

        SetProviderPaymentId(providerPaymentId);

        Status = OperationStatus.Rejected;
    }

    #endregion

    #region Private methods

    private void SetProviderPaymentId(Guid providerPaymentId)
    {
        if (ProviderPaymentId is not null &&
            ProviderPaymentId != providerPaymentId)
        {
            throw new DomainException(
                $"Provider payment id cannot be changed for operation '{OperationId}'.");
        }

        ProviderPaymentId ??= providerPaymentId;
    }

    #endregion
}