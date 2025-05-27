namespace UvA.SecureEduIdInviteProxy.Infrastructre;

/// <summary>
/// Configuration for Azure Monitor Application Insights
/// </summary>
public class AzureMonitorConfig
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "AzureMonitor";
    
    /// <summary>
    /// The Connection String for Azure Application Insights
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}
