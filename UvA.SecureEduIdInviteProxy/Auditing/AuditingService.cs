namespace UvA.SecureEduIdInviteProxy.Auditing;

/// <summary>
/// Implementation of the auditing service that logs to Azure Monitor
/// </summary>
public class AzureMonitorAuditingService(ILogger<AzureMonitorAuditingService> logger) : IAuditingService
{
    /// <inheritdoc />
    public Task LogInviteOperationAsync(string originIpAddress, string roleId, bool isSuccessful, IReadOnlyCollection<string> emailAddresses)
    {
        // Create a structured log with all required audit information
        foreach (var emailAddress in emailAddresses)
        {
            var auditLog = new
            {
                Timestamp = DateTime.UtcNow,
                OriginIpAddress = originIpAddress,
                RoleId = roleId,
                IsSuccessful = isSuccessful,
                EmailAddress = emailAddress,
                OperationType = "InviteOperation"
            };

            // Log as structured data with the appropriate log level based on success
            if (isSuccessful)
            {
                logger.LogInformation("Invite operation completed successfully. {@AuditData}", auditLog);
            }
            else
            {
                logger.LogWarning("Invite operation failed. {@AuditData}", auditLog);
            }
        }

        return Task.CompletedTask;
    }
}