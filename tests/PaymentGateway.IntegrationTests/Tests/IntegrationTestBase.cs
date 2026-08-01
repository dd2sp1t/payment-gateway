using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaymentGateway.Application.BackgroundServices.DispatchOperations;
using PaymentGateway.IntegrationTests.Helpers;
using PaymentGateway.IntegrationTests.PaymentProvider;

namespace PaymentGateway.IntegrationTests.Tests;

[Collection("IntegrationTestsCollection")]
public abstract class IntegrationTestBase : IDisposable
{
    protected HttpClient Client { get; }
    protected IServiceScope Scope { get; }
    protected AssertHelper Assert { get; }

    protected internal PaymentProviderScenarioStore ScenarioStore { get; }

    protected IntegrationTestBase(TestDatabaseFixture fixture)
    {
        Client = fixture.Factory.CreateClient();

        Scope = fixture.Factory.Services.CreateScope();

        ScenarioStore = fixture.Factory.Services.GetRequiredService<PaymentProviderScenarioStore>();
        ScenarioStore.Clear();

        Assert = new AssertHelper(
            TestOptions.Background,
            TestOptions.StabilityDelay);
    }

    public virtual void Dispose()
    {
        ScenarioStore.Clear();

        Scope.Dispose();
    }
}

[CollectionDefinition("IntegrationTestsCollection")]
public sealed class IntegrationTestsCollection : ICollectionFixture<TestDatabaseFixture>
{
}