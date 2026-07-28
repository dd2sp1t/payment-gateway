namespace PaymentGateway.Infrastructure.PaymentProvider.PaymentProviderClients.Testing;

internal static class PaymentProviderScenarioStoreExtensions
{
    public static PaymentProviderScenarioStore SeedRandom(
        this PaymentProviderScenarioStore store,
        int operationCount,
        int maxRetryCount,
        int lastOperationId)
    {
        var random = Random.Shared;

        for (var i = 1; i <= operationCount; i++)
        {
            var builder = store.For($"operation-{lastOperationId + i}");

            var failures = random.Next(1, maxRetryCount + 1);

            switch (random.Next(9))
            {
                case 0:
                    builder
                        .Accepted();
                    break;

                case 1:
                    builder
                        .AcceptedNewPaymentId();
                    break;

                case 2:
                    builder
                        .ServiceUnavailable(failures)
                        .Accepted();
                    break;

                case 3:
                    builder
                        .GatewayTimeout(failures)
                        .Accepted();
                    break;

                case 4:
                    builder
                        .TooManyRequests(failures)
                        .Accepted();
                    break;

                case 5:
                    builder
                        .Timeout(failures)
                        .Accepted();
                    break;

                case 6:
                    builder
                        .SocketError(failures)
                        .Accepted();
                    break;

                case 7:
                    builder
                        .IoError(failures)
                        .Accepted();
                    break;

                case 8:
                    builder
                        .UnexpectedError(failures)
                        .Accepted();
                    break;
            }
        }

        return store;
    }
}