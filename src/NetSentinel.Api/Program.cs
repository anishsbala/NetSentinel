using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using NetSentinel.Api.Data;
using NetSentinel.Api.Models;
using NetSentinel.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddDbContext<NetSentinelDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
builder.Services.AddHealthChecks().AddDbContextCheck<NetSentinelDbContext>();
builder.Services.Configure<SecurityAnalysisOptions>(
    builder.Configuration.GetSection(SecurityAnalysisOptions.SectionName));
builder.Services.AddScoped<IFirewallRuleAnalyzer, FirewallRuleAnalyzer>();
builder.Services.AddScoped<IScanIngestionService, ScanIngestionService>();
builder.Services.AddScoped<IAgentHeartbeatService, AgentHeartbeatService>();

var app = builder.Build();

if (app.Configuration.GetValue("Database:ApplyMigrations", true))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<NetSentinelDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapOpenApi();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
