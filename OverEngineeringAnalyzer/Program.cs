using Microsoft.Build.Locator;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;

 

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"Current Environment: {builder.Environment.EnvironmentName}");


// Load configuration first
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .AddJsonFile(".env.json", optional: true);
var configFiles = new[] {
    Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"),
    Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.{builder.Environment.EnvironmentName}.json"),
    Path.Combine(Directory.GetCurrentDirectory(), ".env.json")
};
// After configuration setup
Console.WriteLine("\nContent of configuration files:");

// Read and display appsettings.json
Console.WriteLine("\nappsettings.json content:");
var appSettings = File.ReadAllText("appsettings.json");
Console.WriteLine(appSettings);

// Read and display appsettings.Development.json
Console.WriteLine("\nappsettings.Development.json content:");
var devSettings = File.ReadAllText("appsettings.Development.json");
Console.WriteLine(devSettings);

// Read and display .env.json
Console.WriteLine("\n.env.json content:");
var envSettings = File.ReadAllText(".env.json");
Console.WriteLine(envSettings);

// Display all configuration values
Console.WriteLine("\nAll Configuration Values:");
foreach (var conf in builder.Configuration.AsEnumerable())
{
    Console.WriteLine($"{conf.Key} = {conf.Value}");
}
// Configure CORS with environment variables
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(corsBuilder =>
    {
        var allowedOrigins = builder.Configuration.GetValue<string>("ALLOWED_ORIGINS")?.Split(',') 
            ?? new[] { "http://localhost:3000" };

        corsBuilder
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configure OpenAI settings
builder.Services.Configure<OpenAIConfig>(options =>
{
      options.ApiKey = builder.Configuration["OpenAI:ApiKey"] ?? 
        throw new InvalidOperationException("OpenAI API Key not configured");
    options.DeploymentName = builder.Configuration["OpenAI:DeploymentName"] ?? 
        throw new InvalidOperationException("OpenAI Deployment Name not configured");
    options.ApiVersion = builder.Configuration["OpenAI:ApiVersion"] ?? 
        throw new InvalidOperationException("OpenAI API Version not configured");
    options.Endpoint = builder.Configuration["OpenAI:Endpoint"] ?? 
        throw new InvalidOperationException("Azure OpenAI Endpoint not configured");
});

// Register services
builder.Services.AddHttpClient();
 
MSBuildLocator.RegisterDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OverEngineeringAnalyzer", Version = "v1" });
    c.CustomOperationIds(apiDesc => apiDesc.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name : null);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

app.Run();