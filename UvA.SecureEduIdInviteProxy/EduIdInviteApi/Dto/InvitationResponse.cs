using System.Text.Json.Serialization;

namespace UvA.SecureEduIdInviteProxy.EduIdInviteApi.Dto;

/// <summary>
/// Represents the response from creating an invitation
/// </summary>
public record InvitationResponse
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; init; }

    /// <summary>
    /// List of recipient invitation URLs
    /// </summary>
    [JsonPropertyName("recipientInvitationURLs")]
    public List<RecipientInvitationURL> RecipientInvitationURLs { get; init; } = null!;
}