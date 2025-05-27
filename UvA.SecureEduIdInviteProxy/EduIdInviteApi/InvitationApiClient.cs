using System.Text.Json;
using UvA.SecureEduIdInviteProxy.EduIdInviteApi.Dto;

namespace UvA.SecureEduIdInviteProxy.EduIdInviteApi;

public interface IInvitationApiClient
{
    /// <summary>
    /// Creates a new invitation
    /// </summary>
    /// <param name="request">The invitation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The invitation response</returns>
    Task<InvitationResponse?> CreateInvitationAsync(CreateInvitation request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Client for the SurfConext Invitation API
/// </summary>
public class InvitationApiClient : IInvitationApiClient
{
    private readonly HttpClient httpClient;
    private readonly ILogger<InvitationApiClient> logger;
    private readonly JsonSerializerOptions jsonOptions;
        
    public static string HttpClientName => "SurfConextInvitationApi";

    /// <summary>
    /// Creates a new instance of the InvitationApiClient
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory</param>
    /// <param name="logger">The logger</param>
    public InvitationApiClient(IHttpClientFactory httpClientFactory, ILogger<InvitationApiClient> logger)
    {
        httpClient = httpClientFactory.CreateClient(HttpClientName);
        this.logger = logger;
        jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }
   
    /// <summary>
    /// Creates a new invitation
    /// </summary>
    /// <param name="request">The invitation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The invitation response</returns>
    public async Task<InvitationResponse?> CreateInvitationAsync(CreateInvitation request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Forwarding invitation creation request for {Count} recipients", request.Invites.Count);
            
        var response = await httpClient.PostAsJsonAsync("/api/external/v1/invitations", request, jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
            
        return await response.Content.ReadFromJsonAsync<InvitationResponse>(jsonOptions, cancellationToken);
    }
}