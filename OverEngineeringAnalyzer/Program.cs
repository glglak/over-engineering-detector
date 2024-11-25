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
     options.OpenAI_ApiKey = builder.Configuration["OpenAI_ApiKey"] ?? builder.Configuration["OpenAI:ApiKey"] ?? 
        throw new InvalidOperationException("OpenAI API Key not configured");
    options.OpenAI_DeploymentName = builder.Configuration["OpenAI_DeploymentName"] ?? builder.Configuration["OpenAI:DeploymentName"] ?? 
        throw new InvalidOperationException("OpenAI Deployment Name not configured");
    options.OpenAI_ApiVersion = builder.Configuration["OpenAI_ApiVersion"] ?? builder.Configuration["OpenAI:ApiVersion"] ?? 
        throw new InvalidOperationException("OpenAI API Version not configured");
    options.OpenAI_Endpoint = builder.Configuration["OpenAI_Endpoint"] ?? builder.Configuration["OpenAI:Endpoint"] ?? 
        throw new InvalidOperationException("Azure OpenAI Endpoint not configured");

    // Map the Azure App Service setting names to the properties used in the rest of the app
    options.ApiKey = options.OpenAI_ApiKey;
    options.DeploymentName = options.OpenAI_DeploymentName;
    options.ApiVersion = options.OpenAI_ApiVersion;
    options.Endpoint = options.OpenAI_Endpoint;
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