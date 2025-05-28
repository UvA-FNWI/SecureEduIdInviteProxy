using System.ComponentModel.DataAnnotations;

namespace UvA.SecureEduIdInviteProxy.Infrastructre;

/// <summary>
/// Configuration for the EduId invitation API
/// </summary>
public class EduIdConfig
{
    public const string SectionName = "EduId";
    
    /// <summary>
    /// The base URL of the SurfConext Invitation API
    /// </summary>
    [Required(ErrorMessage = "The SurfConext Invitation API URL is required")]
    [Url(ErrorMessage = "The SurfConext Invitation API URL must be a valid URL")]
    public string InvitationApiUrl { get; init; } = null!;
    
    /// <summary>
    /// The API token for authenticating with the SurfConext Invitation API
    /// </summary>
    [Required(ErrorMessage = "The SurfConext Invitation API token is required")]
    public string InvitationApiToken { get; init; } = null!;
    
    [Required(ErrorMessage = "At least one role-specific API token must be configured")]
    public Dictionary<string, string> RoleTokens { get; init; } = new();
    
    [Required(ErrorMessage = "At least one role-id must be configured")]
    public Dictionary<string, string> RoleIds { get; set; } = new();
}