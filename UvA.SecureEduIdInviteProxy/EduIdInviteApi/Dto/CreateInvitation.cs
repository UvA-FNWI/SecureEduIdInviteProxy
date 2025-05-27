using System.Text.Json.Serialization;

namespace UvA.SecureEduIdInviteProxy.EduIdInviteApi.Dto;

/// <summary>
/// Represents a request to create a new invitation
/// </summary>
public class CreateInvitation
{
    /// <summary>
    /// The intended authority for the invitation
    /// </summary>
    [JsonPropertyName("intendedAuthority")]
    public string IntendedAuthority { get; init; } = null!;

    /// <summary>
    /// Optional message to include in the invitation
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = null!;

    /// <summary>
    /// Language for the invitation (en or nl)
    /// </summary>
    [JsonPropertyName("language")]
    public string Language { get; init; } = null!;

    /// <summary>
    /// Whether to enforce email equality
    /// </summary>
    [JsonPropertyName("enforceEmailEquality")]
    public bool? EnforceEmailEquality { get; init; }

    /// <summary>
    /// Whether to restrict to eduID only
    /// </summary>
    [JsonPropertyName("eduIDOnly")]
    public bool? EduIDOnly { get; init; }

    /// <summary>
    /// Whether to include guest role
    /// </summary>
    [JsonPropertyName("guestRoleIncluded")]
    public bool? GuestRoleIncluded { get; init; }

    /// <summary>
    /// Whether to suppress sending emails
    /// </summary>
    [JsonPropertyName("suppressSendingEmails")]
    public bool? SuppressSendingEmails { get; init; }

    /// <summary>
    /// List of email addresses to invite
    /// </summary>
    [JsonPropertyName("invites")]
    public List<string> Invites { get; init; } = null!;

    /// <summary>
    /// List of role identifiers
    /// </summary>
    [JsonPropertyName("roleIdentifiers")]
    public List<long> RoleIdentifiers { get; init; } = null!;

    /// <summary>
    /// Organization GUID
    /// </summary>
    [JsonPropertyName("organizationGUID")]
    public string OrganizationGUID { get; init; } = null!;

    /// <summary>
    /// Expiry date for the role
    /// </summary>
    [JsonPropertyName("roleExpiryDate")]
    public DateTime? RoleExpiryDate { get; init; }

    /// <summary>
    /// Expiry date for the invitation
    /// </summary>
    [JsonPropertyName("expiryDate")]
    public DateTime ExpiryDate { get; init; }
}