using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TaskService.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TaskServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TaskServiceContext") ?? throw new InvalidOperationException("Connection string 'TaskServiceContext' not found.")));

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
                opts.Endpoint = new Uri("http://localhost:4317");
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
