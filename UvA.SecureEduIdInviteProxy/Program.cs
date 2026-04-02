using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.OpenApi.Models;
using UvA.SecureEduIdInviteProxy.Auditing;
using UvA.SecureEduIdInviteProxy.EduIdInviteApi;
using UvA.SecureEduIdInviteProxy.Endpoints;
using UvA.SecureEduIdInviteProxy.Infrastructre;
using Serilog;
using Serilog.Filters;

Console.WriteLine("SecureEduIdInviteProxy initializing");

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

builder.Services.AddApplicationInsightsTelemetry();

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ApplicationName", "EduIdInviteProxy")
        .WriteTo.Logger(lc => lc
            .Filter.ByExcluding(e => e.Properties.ContainsKey("AuditData"))
            .WriteTo.Console(outputTemplate:"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
        .WriteTo.ApplicationInsights(
            services.GetRequiredService<TelemetryConfiguration>(),
            TelemetryConverter.Traces);
});

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SecureEduIdInviteProxy API", Version = "v1" });
});
builder.Services.AddHttpContextAccessor();

// Register the SurfConext Invitation API client
builder.Services.AddInvitationApiClient(builder.Configuration);

// Add validation for configuration at startup
builder.Services.AddOptions<EduIdConfig>()
    .BindConfiguration(EduIdConfig.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart()
    .PostConfigure(config =>
    {
        // Swap the key and value so we can look up the role name based on the role id
        config.RoleIds = config.RoleIds.ToDictionary(x => x.Value, x => x.Key);
    });

// Register auditing services
builder.Services.AddScoped<AuditingService>();
builder.Services.AddScoped<IAuditingService, AuditingService>();

var app = builder.Build();

app.Logger.LogInformation("SecureEduIdInviteProxy starting up");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SecureEduIdInviteProxy API v1"));
}

// Map all invitation endpoints
app.MapInvitationEndpoints();

app.Logger.LogInformation("SecureEduIdInviteProxy running");
app.Run();

