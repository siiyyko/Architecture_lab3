using Microsoft.Extensions.Configuration;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var allowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

var serviceName = "ApiGateway";
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
    );

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();

app.UseCors(allowSpecificOrigins);

app.MapGet("/", () => "Hello World!");

await app.UseOcelot();

app.Run();
