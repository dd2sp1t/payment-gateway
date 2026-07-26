using PaymentGateway.Api.ExceptionHandling;
using PaymentGateway.Api.Serialization;
using PaymentGateway.Application;
using PaymentGateway.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler();

app.MapControllers();

app.Run();