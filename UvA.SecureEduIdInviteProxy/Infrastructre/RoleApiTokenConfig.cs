using System.ComponentModel.DataAnnotations;

namespace UvA.SecureEduIdInviteProxy.Infrastructre;

/// <summary>
/// Configuration for role-specific API tokens
/// </summary>
public class RoleApiTokenConfig
{
    public const string SectionName = "RoleApiTokens";
    
    /// <summary>
    /// Dictionary mapping role IDs to their corresponding API tokens
    /// </summary>
    [Required(ErrorMessage = "At least one role-specific API token must be configured")]
    public Dictionary<string, string> Tokens { get; set; } = new();
    
    /// <summary>
    /// Validates that the configuration has at least one token
    /// </summary>
    /// <param name="validationContext">The validation context</param>
    /// <returns>The validation result</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Tokens.Count == 0)
        {
            yield return new ValidationResult(
                "At least one role-specific API token must be configured",
                new[] { nameof(Tokens) });
        }
    }
}
