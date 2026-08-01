using PaymentGateway.IntegrationTests.Helpers;

namespace PaymentGateway.IntegrationTests.Tests.Scenarios;

public class DuplicateOperationTests : IntegrationTestBase
{
    public DuplicateOperationTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task OperationId_ShouldBeUnique_WhenCreatingOperation()
    {
        // arrange
        var operationId = $"op-create-duplicate-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        // act
        var firstResponse = await Client.CreateOperationAsync(
            operationId,
            amount,
            currency,
            description);

        await AssertHelper.AssertOperationCreatedAsync(
            firstResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        var duplicateResponse = await Client.CreateOperationAsync(
            operationId,
            amount,
            currency,
            description);

        // assert
        await AssertHelper.AssertConflictAsync(duplicateResponse);
    }
}