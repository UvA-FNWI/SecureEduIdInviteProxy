using System.Net;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using UvA.SecureEduIdInviteProxy.Infrastructre;

namespace UvA.SecureEduIdInviteProxy.EduIdInviteApi;

/// <summary>
/// Extension methods for registering SurfConext Invitation API client
/// </summary>
public static class InvitationApiClientExtensions
{
    /// <summary>
    /// Add SurfConext invitation API client to the service collection with configurable HTTP client settings
    /// </summary>
    /// <param name="services">The service collection to add the clients to</param>
    /// <param name="configuration">Configuration to use</param>
    /// <param name="configureClient">Optional configuration action for customizing the HttpClient</param>
    /// <returns>The updated service collection with registered SurfConext invitation clients</returns>
    public static IServiceCollection AddInvitationApiClient(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<HttpClient>? configureClient = null)
    {
        // Register the configuration
        services.Configure<EduIdConfig>(configuration.GetSection(EduIdConfig.SectionName));
        
        // Register the HTTP client with resilience policies
        services.AddHttpClient<InvitationApiClient>(InvitationApiClient.HttpClientName, (serviceProvider, client)
                =>
            {
                // Get the configuration from the service provider
                var eduIdConfig = serviceProvider.GetRequiredService<IOptions<EduIdConfig>>().Value;
                
                if(string.IsNullOrEmpty(eduIdConfig.InvitationApiUrl))
                    throw new InvalidOperationException("The SurfConext Invitation API URL is not configured");
                if(string.IsNullOrEmpty(eduIdConfig.InvitationApiToken))
                    throw new InvalidOperationException("The SurfConext Invitation API token is not configured");

                client.BaseAddress = new Uri(eduIdConfig.InvitationApiUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("X-API-TOKEN", eduIdConfig.InvitationApiToken);

                // Allow additional configuration
                configureClient?.Invoke(client);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy())
            .AddPolicyHandler(GetTimeoutPolicy());

        // Register the client
        services.AddTransient<IInvitationApiClient, InvitationApiClient>();

        return services;
    }

    /// <summary>
    /// Creates a retry policy for transient HTTP errors
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    /// <summary>
    /// Creates a circuit breaker policy to prevent overwhelming a failing service
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Creates a timeout policy to prevent long-running requests
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));
    }
}