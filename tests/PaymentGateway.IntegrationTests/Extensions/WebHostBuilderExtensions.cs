using Microsoft.AspNetCore.Hosting;

namespace PaymentGateway.IntegrationTests.Extensions;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder ConfigureSerilogForTests(this IWebHostBuilder builder)
    {
        builder.UseSetting("Serilog:Properties:ContainerName", "integration-tests");

        return builder;
    }
}
