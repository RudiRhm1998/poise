using System.Reflection;
using Microsoft.OpenApi.Models;
using poise.Middleware;
using poise.Startup;
using Serilog;
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(k =>
{
    k.AddServerHeader = false;
});

builder.Logging.ClearProviders();
var excludedLoggingContexts = new[]
    { "Microsoft.AspNetCore.Hosting.Diagnostics", "Microsoft.AspNetCore.Mvc.Infrastructure" };
var lc = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] - {SourceContext}.{Method} - {Message:lj}{NewLine}{Exception}")
    .Filter.ByExcluding(x =>
        x.Properties.TryGetValue("SourceContext", out var sc) &&
        excludedLoggingContexts.Any(x => sc.ToString().Contains(x)))
    .CreateLogger();

builder.Logging.AddSerilog(lc);

builder.Services.AddControllers(x => { x.Filters.Add<ValidationErrorExceptionFilter>(); })
    .AddJsonOptions(o => { o.JsonSerializerOptions.PropertyNameCaseInsensitive = true; });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.OperationFilter<AuthorizationOperationFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
