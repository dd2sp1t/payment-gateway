using PaymentGateway.Api.ExceptionHandling;
using PaymentGateway.Api.Serialization;
using PaymentGateway.Application;
using PaymentGateway.Infrastructure;
using PaymentGateway.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

builder.Services
    .AddProblemDetails()
    .AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new UpperCaseEnumConverterFactory());
    });

builder.Services
    .AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

Log.Information("Application built.");

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler();

app.MapControllers();

app.MapHealthChecks("/health");
app.MapPrometheusScrapingEndpoint();

try
{
    Log.Information("Applying database migrations.");

    await app.Services.ApplyMigrationsAsync();

    Log.Information("Database migrations applied.");

    Log.Information("Starting web host.");

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}