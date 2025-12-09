using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheAnalyzer.Interfaces;
using TheAnalyzer.Services;
using TheAnalyzer.Repositories;
using Amazon.Comprehend;
using Amazon.DynamoDBv2;
using Amazon.S3;

namespace TheAnalyzer;

/// <summary>
/// Configures dependency injection for Lambda functions
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Configures services for dependency injection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // Add logging services with console provider
        services.AddLogging(configure =>
        {
            configure.AddConsole();
            configure.SetMinimumLevel(LogLevel.Information);
        });
        
        // Explicitly register generic logger to prevent DI resolution errors
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        
        // Register AWS clients
        services.AddSingleton<IAmazonComprehend, AmazonComprehendClient>();
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
        services.AddSingleton<IAmazonS3, AmazonS3Client>();
        services.AddSingleton<Amazon.BedrockRuntime.IAmazonBedrockRuntime, Amazon.BedrockRuntime.AmazonBedrockRuntimeClient>();
        
        // Register helper services
        services.AddSingleton<RegexEntityExtractor>();
        
        // Register services
        services.AddSingleton<IAnalyzeService, ComprehendAnalyzeService>();
        
        // Register repositories
        services.AddSingleton<IProfileRepository, ProfileRepository>();
        
        return services;
    }
}
