using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TaskService.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TaskServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TaskServiceContext") ?? throw new InvalidOperationException("Connection string 'TaskServiceContext' not found.")));

var otlpEndpoint = builder.Configuration["OTLP_ENDPOINT_URL"] ?? "http://localhost:4317";

// Add services to the container.

var serviceName = "TaskService";

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddSource(serviceName)
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName: serviceName))

            .AddAspNetCoreInstrumentation()

            .AddConsoleExporter()

            .AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(otlpEndpoint);
                opts.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            })

     .AddEntityFrameworkCoreInstrumentation()
     .AddRabbitMQInstrumentation()
    );

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<TaskService.Services.MessageBusConsumer>();

using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<TaskService.Data.TaskServiceContext>(); 
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("--> Applying EF Core Migrations...");
            dbContext.Database.Migrate();
            Console.WriteLine("--> Migrations Applied.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> An error occurred while applying migrations: {ex.Message}");
    }
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
