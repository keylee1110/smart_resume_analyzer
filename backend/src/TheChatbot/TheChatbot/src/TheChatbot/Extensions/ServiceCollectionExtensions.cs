using Amazon.DynamoDBv2;
using Amazon.BedrockRuntime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheChatbot.Data;
using TheChatbot.Services;
using System;

namespace TheChatbot;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add AWS SDK clients
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
        services.AddSingleton<IAmazonBedrockRuntime, AmazonBedrockRuntimeClient>();

        // Add configuration (important for reading environment variables like TABLE_NAME)
        services.AddSingleton<IConfiguration>(sp =>
        {
            var configurationBuilder = new ConfigurationBuilder();
            // Environment variables are automatically available via Environment.GetEnvironmentVariable
            // No need to explicitly add them to configuration
            return configurationBuilder.Build();
        });

        // Add repositories and services
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IChatHistoryRepository, ChatHistoryRepository>();
        services.AddScoped<IBedrockChatService, BedrockChatService>();

        return services;
    }
}
