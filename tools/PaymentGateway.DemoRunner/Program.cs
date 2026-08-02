using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PaymentGateway.DemoRunner;
using PaymentGateway.DemoRunner.Scenarios;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        builder.Services.Configure<DemoRunnerOptions>(
            configuration.GetSection(nameof(DemoRunnerOptions)));

        builder.Services.AddHttpClient<PaymentGatewayClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<DemoRunnerOptions>>().Value;
            client.BaseAddress = new Uri(options.GatewayUrl);
        });

        builder.Services.AddTransient<IScenario, BasicFlowScenario>();
        builder.Services.AddTransient<IScenario, ConcurrentSubmitScenario>();
        builder.Services.AddTransient<IScenario, DuplicateCreateScenario>();
        builder.Services.AddTransient<IScenario, ValidationScenario>();

        using var host = builder.Build();

        var scenario = ParseScenario(args);

        await RunScenarioAsync(
            host.Services.GetRequiredService<IEnumerable<IScenario>>(),
            scenario);
    }

    private static DemoScenario? ParseScenario(string[] args)
    {
        if (args.Length != 1 ||
            Enum.TryParse<DemoScenario>(args[0], true, out var scenario) == false)
        {
            PrintUsage();
            return null;
        }

        return scenario;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("""
Usage:

  dotnet run --project tools/PaymentGateway.DemoRunner -- basic
  dotnet run --project tools/PaymentGateway.DemoRunner -- concurrent
  dotnet run --project tools/PaymentGateway.DemoRunner -- duplicate
  dotnet run --project tools/PaymentGateway.DemoRunner -- validation
  dotnet run --project tools/PaymentGateway.DemoRunner -- all
""");
    }

    private static async Task RunScenarioAsync(
        IEnumerable<IScenario> scenarios,
        DemoScenario? scenario,
        CancellationToken cancellationToken = default)
    {
        if (scenario is null)
        {
            return;
        }

        if (scenario == DemoScenario.All)
        {
            foreach (var current in scenarios)
            {
                await current.RunAsync(cancellationToken);
            }

            return;
        }

        var target = scenarios.Single(
            x => string.Equals(
                x.Name,
                scenario.ToString(),
                StringComparison.OrdinalIgnoreCase));

        await target.RunAsync(cancellationToken);
    }
}