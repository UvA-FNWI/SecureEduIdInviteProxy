using Microsoft.Extensions.Options;
using UvA.SecureEduIdInviteProxy.Auditing;
using UvA.SecureEduIdInviteProxy.EduIdInviteApi;
using UvA.SecureEduIdInviteProxy.Endpoints;
using UvA.SecureEduIdInviteProxy.Infrastructre;

Console.WriteLine("UvA.SecureEduIdInviteProxy starting up");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();


// Register the SurfConext Invitation API client
builder.Services.AddInvitationApiClient(builder.Configuration);

// Register the RoleApiTokenConfig
builder.Services.Configure<RoleApiTokenConfig>(builder.Configuration.GetSection(RoleApiTokenConfig.SectionName));

// Add validation for configuration at startup
builder.Services.AddOptions<EduIdConfig>()
    .BindConfiguration(EduIdConfig.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Configure Azure Monitor for application insights and audit logging
builder.Services.Configure<AzureMonitorConfig>(
    builder.Configuration.GetSection(AzureMonitorConfig.SectionName));

// Add Application Insights telemetry if configured
if (!string.IsNullOrEmpty(builder.Configuration.GetSection(AzureMonitorConfig.SectionName)["ConnectionString"]))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = builder.Configuration.GetSection(AzureMonitorConfig.SectionName)["ConnectionString"];
    });
}

// Register auditing services
builder.Services.AddScoped<AzureMonitorAuditingService>();
builder.Services.AddScoped<IAuditingService>(sp =>
{
    var config = sp.GetRequiredService<IOptions<AzureMonitorConfig>>();
    return sp.GetRequiredService<AzureMonitorAuditingService>();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map all invitation endpoints
app.MapInvitationEndpoints();

Console.WriteLine("UvA.SecureEduIdInviteProxy started");

app.Run();

