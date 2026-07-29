using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.IntegrationTests.PaymentProvider;

namespace PaymentGateway.IntegrationTests.Tests;

[Collection("IntegrationTestsCollection")]
public abstract class IntegrationTestBase : IDisposable
{
    protected HttpClient Client { get; }

    protected IServiceScope Scope { get; }

    protected PaymentProviderScenarioStore ScenarioStore { get; }

    protected IntegrationTestBase(TestDatabaseFixture fixture)
    {
        Client = fixture.Factory.CreateClient();

        Scope = fixture.Factory.Services.CreateScope();

        ScenarioStore = Scope.ServiceProvider.GetRequiredService<PaymentProviderScenarioStore>();

        ScenarioStore.Clear();
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