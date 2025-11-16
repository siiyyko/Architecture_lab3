using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using UserService.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<UserServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserServiceContext") ?? throw new InvalidOperationException("Connection string 'UserServiceContext' not found.")));

// Add services to the container.

var serviceName = "UserService";

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
    );


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
