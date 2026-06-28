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

    /// <summary>
    /// Get user/role mappings
    /// </summary>
    /// <param name="roleId">Target role ID</param>
    /// <param name="guests">If true, return guests. Otherwise managers are returned</param>
    /// <param name="query">Query string, e.g. an email address</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="ct"></param>
    Task<UserRolesResponse> FindUserRoles(int roleId, bool guests, string query, int pageNumber, CancellationToken ct);

    /// <summary>
    /// Update the end date of a user role
    /// </summary>
    /// <param name="userRoleId">Target user role ID</param>
    /// <param name="endDate">New end date</param>
    /// <param name="ct"></param>
    Task UpdateUserRole(int userRoleId, DateTime endDate, CancellationToken ct);
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
    
    public async Task<UserRolesResponse> FindUserRoles(int roleId, bool guests, string query, int pageNumber,
        CancellationToken ct)
    {
        var response = await httpClient.GetFromJsonAsync<UserRolesResponse>(
                $"/api/external/v1/user_roles/search/{roleId}/{guests}?query={query}&pageNumber={pageNumber}", ct);

        return response ?? throw new Exception("No response from invitation service");
    }

    /// <summary>
    /// Update the end date of a user role
    /// </summary>
    /// <param name="userRoleId">Target user role ID</param>
    /// <param name="endDate">New end date</param>
    /// <param name="ct"></param>
    public async Task UpdateUserRole(int userRoleId, DateTime endDate, CancellationToken ct)
    {
        var response = await httpClient.PutAsJsonAsync("/api/external/v1/user_roles", new
            {
                userRoleId, 
                endDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            },
            jsonOptions, ct);
        
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to update user role: {await response.Content.ReadAsStringAsync(ct)}",
                null, response.StatusCode);
        response.EnsureSuccessStatusCode();
    }
}