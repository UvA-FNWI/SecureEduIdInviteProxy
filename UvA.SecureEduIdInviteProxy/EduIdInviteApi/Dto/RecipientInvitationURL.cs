using System.Text.Json.Serialization;

namespace UvA.SecureEduIdInviteProxy.EduIdInviteApi.Dto;

/// <summary>
/// Represents a recipient and their invitation URL
/// </summary>
public class RecipientInvitationURL
{
    /// <summary>
    /// Email address of the recipient
    /// </summary>
    [JsonPropertyName("recipient")]
    public string Recipient { get; init; } = null!;

    /// <summary>
    /// URL for the invitation
    /// </summary>
    [JsonPropertyName("invitationURL")]
    public string InvitationURL { get; init; } = null!;
}