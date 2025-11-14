using Sales.Api.Middleware;
using Sales.Application.Common.Correlation;
using Sales.Application.Common.Messaging;
using Sales.Application.Sales;
using Sales.Infrastructure.Messaging;
using Sales.Infrastructure.Persistence;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(new CompactJsonFormatter()));

    builder.Services.Configure<RabbitMqOptions>(
        builder.Configuration.GetSection(RabbitMqOptions.SectionName));

    builder.Services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
    builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();
    builder.Services.AddSingleton<ISaleRepository, InMemorySaleRepository>();
    builder.Services.AddScoped<ISaleService, SaleService>();

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Controllers
    builder.Services.AddControllers();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
