using Microsoft.Extensions.Options;
using Serilog;
using System.Text.RegularExpressions;
using UvA.SecureEduIdInviteProxy.Infrastructre;

namespace UvA.SecureEduIdInviteProxy.Endpoints;

public partial class ApiKeyService(IOptions<EduIdConfig> eduIdConfig, IHttpContextAccessor httpContextAccessor)
{
    [GeneratedRegex(@"^[A-Za-z0-9\/!\-=\?]+$")]
    private static partial Regex ApiTokenRegex();

    private const int MaxTokenLength = 512;
    
    public bool CheckKey(long roleId)
    {
        var apiToken = httpContextAccessor.HttpContext?.Request.Headers["X-API-TOKEN"].FirstOrDefault();
        
        // Check that the token is valid (not empty and only contains alphanumeric chars or a dash)
        if (string.IsNullOrEmpty(apiToken) ||
            apiToken.Length > MaxTokenLength ||
            !ApiTokenRegex().IsMatch(apiToken))
        {
            Log.Debug("Invalid API token received");
            return false;
        }
            
        // Get the name from the request
        if (!eduIdConfig.Value.RoleIds.TryGetValue(roleId.ToString(), out var roleName))
        {
            Log.Warning("Role ID {RoleId} is not configured", roleId);
            return false;
        }
        // Get the token based on the rolename
        if (!eduIdConfig.Value.RoleTokens.TryGetValue(roleName, out var expectedToken))
        {
            Log.Error("Role name {RoleName} is not configured", roleName);
            return false;
        }

        // Verify token
        if (apiToken != expectedToken)
        {
            Log.Warning("API token '{ApiToken}... does not match the configured token for role {RoleId}", apiToken[..Math.Min(4,apiToken.Length)], roleId);
            return false;
        }

        return true;
    }
}