using Microsoft.AspNetCore.Mvc;
using UvA.SecureEduIdInviteProxy.EduIdInviteApi;

namespace UvA.SecureEduIdInviteProxy.Endpoints;

public static partial class InvitationEndpoints
{
    private static async Task<IResult> SearchUserRoles(
        int roleId,
        bool guests,
        [FromQuery] string query,
        ApiKeyService apiKeyService,
        IInvitationApiClient invitationApiClient,
        CancellationToken cancellationToken,
        [FromQuery] int pageNumber = 0
    )
    {
        if (!apiKeyService.CheckKey(roleId))
            return Results.Unauthorized();
        return Results.Ok(await invitationApiClient.FindUserRoles(roleId, guests, query, pageNumber, cancellationToken));
    }
}