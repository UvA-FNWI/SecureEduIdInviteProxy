using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using System.Text.RegularExpressions;
using UvA.SecureEduIdInviteProxy.Auditing;
using UvA.SecureEduIdInviteProxy.EduIdInviteApi;
using UvA.SecureEduIdInviteProxy.EduIdInviteApi.Dto;
using UvA.SecureEduIdInviteProxy.Infrastructre;

namespace UvA.SecureEduIdInviteProxy.Endpoints;

/// <summary>
/// Endpoints for handling invitation requests
/// </summary>
public static partial class InvitationEndpoints
{
    [GeneratedRegex(@"^[a-zA-Z0-9-]+$")]
    private static partial Regex ApiTokenRegex();

    private const int MaxTokenLength = 128;
    
    /// <summary>
    /// Maps all invitation endpoints to the application
    /// </summary>
    /// <param name="app">The web application to map endpoints to</param>
    /// <returns>The web application with mapped endpoints</returns>
    public static WebApplication MapInvitationEndpoints(this WebApplication app)
    {
        app.MapPost("/api/external/v1/invitations", CreateInvitation)
            .WithName("CreateInvitation")
            .WithOpenApi();
            
        return app;
    }
    
    /// <summary>
    /// Handles the creation of invitations by proxying requests to the SurfConext Invitation API
    /// </summary>
    private static async Task<IResult> CreateInvitation(
        [FromBody] CreateInvitation request,
        [FromHeader(Name = "X-API-TOKEN")] string apiToken,
        IOptions<EduIdConfig> eduIdConfig,
        IInvitationApiClient invitationApiClient,
        IAuditingService auditingService,
        IHttpContextAccessor httpContextAccessor,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check that the token is valid (not empty and only contains alphanumeric chars or a dash)
            if (string.IsNullOrEmpty(apiToken) ||
                apiToken.Length > MaxTokenLength ||
                !ApiTokenRegex().IsMatch(apiToken))
            {
                Log.Debug("Invalid API token received");
                return Results.Unauthorized();
            }

            // Verify that only one role ID is provided
            if (request.RoleIdentifiers.Count != 1)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Invalid request",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = "One and only one role identifier must be provided"
                });
            }
            var roleId = request.RoleIdentifiers.First().ToString();
           
            Log.Information("Received invitation request for {Count} recipients with role ID: {RoleId}", request.Invites.Count, roleId);
            
            // Get the role ID from the request
            if (!eduIdConfig.Value.RoleApiTokens.TryGetValue(roleId, out var expectedToken))
            {
                Log.Information("No API token configured for role ID {RoleId}", roleId);
                return Results.Unauthorized();
            }

            // Verify token
            if (apiToken != expectedToken)
            {
                Log.Warning("API token {ApiToken} does not match the configured token for role {RoleId}", apiToken, roleId);
                return Results.Unauthorized();
            }

            var sourceIp = httpContextAccessor.HttpContext!.Connection.RemoteIpAddress!.ToString();
            try
            {
                // Forward the request to the actual API using the correct token
                var response = await invitationApiClient.CreateInvitationAsync(request, cancellationToken);
                if (response is null)
                {
                    Log.Warning("No response from invitation service, request: {@Request}", request);;
                    return Results.Problem(
                        title: "Error from invitation service",
                        detail: "No response from invitation service",
                        statusCode: (int)HttpStatusCode.InternalServerError);
                }

                // Log the operation to the audit log
                await auditingService.LogInviteOperationAsync(sourceIp, roleId, true, request.Invites);

                return Results.Ok(response);
            }
           
            catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
            {
                // Propagate the status code from the downstream service
                Log.Warning(ex, "Invitation service returned error {StatusCode}, {@Request}", ex.StatusCode.Value, request);

                // Log the operation to the audit log
                await auditingService.LogInviteOperationAsync(sourceIp, roleId, false, request.Invites);
                
                return Results.Problem(
                    title: "Error from invitation service",
                    detail: ex.Message,
                    statusCode: (int)ex.StatusCode.Value);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing invitation request: {@Request}", request);;
            return Results.Problem(
                title: "Error processing invitation request",
                statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }
}
