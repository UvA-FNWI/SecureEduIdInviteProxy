using System.Net;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using UvA.SecureEduIdInviteProxy.Auditing;
using UvA.SecureEduIdInviteProxy.EduIdInviteApi;
using UvA.SecureEduIdInviteProxy.EduIdInviteApi.Dto;

namespace UvA.SecureEduIdInviteProxy.Endpoints;

/// <summary>
/// Endpoints for handling invitation requests
/// </summary>
public static partial class InvitationEndpoints
{
    /// <summary>
    /// Maps all invitation endpoints to the application
    /// </summary>
    /// <param name="app">The web application to map endpoints to</param>
    /// <returns>The web application with mapped endpoints</returns>
    public static WebApplication MapInvitationEndpoints(this WebApplication app)
    {
        app.MapPost("/api/external/v1/invitations", CreateInvitation)
            .WithName("CreateInvitation");

        app.MapGet("/api/external/v1/user_roles/search/{roleId}/{guests}", SearchUserRoles)
            .WithName("SearchUserRoles");

        app.MapPut("/api/proxy/v1/extend/{roleId}", UpdateUserRole)
            .WithName("UpdateUserRole");
            
        return app;
    }
    
    /// <summary>
    /// Handles the creation of invitations by proxying requests to the SurfConext Invitation API
    /// </summary>
    private static async Task<IResult> CreateInvitation(
        [FromBody] CreateInvitation request,
        IInvitationApiClient invitationApiClient,
        IAuditingService auditingService,
        IHttpContextAccessor httpContextAccessor,
        ApiKeyService apiKeyService,
        CancellationToken cancellationToken)
    {
        try
        {
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
            var roleId = request.RoleIdentifiers.First();

            if (!apiKeyService.CheckKey(roleId))
                return Results.Unauthorized();
            
            Log.Debug("Received invitation request for {Count} recipients with role ID: {RoleId}", request.Invites.Count, roleId);

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
                await auditingService.LogInviteOperationAsync(sourceIp, roleId.ToString(), true, request.Invites);
                Log.Information("New invitation successfully created for {Count} recipients", request.Invites.Count);
                return Results.Ok(response);
            }
           
            catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
            {
                // Propagate the status code from the downstream service
                Log.Warning(ex, "Invitation service returned error {StatusCode}, {@Request}", ex.StatusCode.Value, request);

                // Log the operation to the audit log
                await auditingService.LogInviteOperationAsync(sourceIp, roleId.ToString(), false, request.Invites);
                
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
