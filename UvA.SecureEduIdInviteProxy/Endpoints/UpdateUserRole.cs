using Microsoft.AspNetCore.Mvc;
using UvA.SecureEduIdInviteProxy.EduIdInviteApi;

namespace UvA.SecureEduIdInviteProxy.Endpoints;

public partial class InvitationEndpoints
{
    public record UpdateUserRoleRequest(string Query, DateTime EndDate);
    
    /// <summary>
    /// Updates a user role by query string. Returns an error unless exactly one result is returned.
    /// </summary>
    /// <remarks>
    /// We cannot update by user role ID because in that case we can't check if the role matches because there is no
    /// endpoint to retrieve a user role by ID.
    /// </remarks>
    public static async Task<IResult> UpdateUserRole(
        int roleId,
        [FromBody] UpdateUserRoleRequest request,
        IInvitationApiClient invitationApiClient,
        ApiKeyService apiKeyService,
        CancellationToken ct
    )
    {
        if (!apiKeyService.CheckKey(roleId))
            return Results.Unauthorized();

        var result = await invitationApiClient.FindUserRoles(roleId, true, request.Query, 0, ct);
        if (result.Content.Length != 1)
            return Results.BadRequest(new
            {
                error = "Unexpected result count",
                count = result.Content.Length
            });

        await invitationApiClient.UpdateUserRole(result.Content[0].Id, request.EndDate, ct);

        return Results.Ok();
    }
}