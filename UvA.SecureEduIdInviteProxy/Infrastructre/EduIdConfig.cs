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
    public string InvitationApiUrl { get; set; } = null!;
    
    /// <summary>
    /// The API token for authenticating with the SurfConext Invitation API
    /// </summary>
    [Required(ErrorMessage = "The SurfConext Invitation API token is required")]
    public string InvitationApiToken { get; set; } = null!;
}