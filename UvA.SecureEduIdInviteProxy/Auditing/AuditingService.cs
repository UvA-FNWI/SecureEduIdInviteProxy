using Serilog;

namespace UvA.SecureEduIdInviteProxy.Auditing;

/// <summary>
/// Implementation of the auditing service that logs to Azure Monitor
/// </summary>
public class AuditingService : IAuditingService
{
    /// <inheritdoc />
    public Task LogInviteOperationAsync(string sourceIpAddress, string roleId, bool isSuccessful, IReadOnlyCollection<string> emailAddresses)
    {
        // Create a structured log with all required audit information
        foreach (var emailAddress in emailAddresses)
        {
            var auditLog = new
            {
                Timestamp = DateTime.UtcNow,
                SourceIpAddress = sourceIpAddress,
                RoleId = roleId,
                IsSuccessful = isSuccessful,
                EmailAddress = emailAddress,
                OperationType = "InviteOperation",
                Category = "AuditLog"
            };
            
            // Log as structured data with the appropriate log level based on success
            if (isSuccessful)
            {
                Log.Information("Invite operation completed successfully. {@AuditData}", auditLog);
            }
            else
            {
                Log.Warning("Invite operation failed. {@AuditData}", auditLog);
            }
        }

        return Task.CompletedTask;
    }
}