using System.Text.Json.Serialization;

namespace UvA.SecureEduIdInviteProxy.EduIdInviteApi.Dto;

public record UserRolesResponse(UserRole[] Content, int TotalPages);

public record UserRole(
    [property: JsonPropertyName("user_id")] int UserId,
    long EndDate,
    string Email,
    int Id,
    long CreatedAt
);