namespace UvA.SecureEduIdInviteProxy.Auditing;

/// <summary>
/// Interface for audit logging service
/// </summary>
public interface IAuditingService
{
    /// <summary>
    /// Logs an invite operation to the audit log
    /// </summary>
    /// <param name="originIpAddress">The IP address of the request origin</param>
    /// <param name="roleId">The role ID associated with the invite</param>
    /// <param name="isSuccessful">Flag indicating if the operation was successful</param>
    /// <param name="emailAddresses">A list of email addresses to invite</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task LogInviteOperationAsync(string originIpAddress, string roleId, bool isSuccessful,  IReadOnlyCollection<string> emailAddresses);
}