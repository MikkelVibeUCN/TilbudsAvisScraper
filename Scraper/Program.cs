using Hangfire;
using Hangfire.PostgreSql;
using Scraper;

string ReadSecret(string path)
{
    return File.Exists(path)
        ? File.ReadAllText(path).Trim()
        : throw new Exception($"Secret file not found: {path}");
}

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:8080");

// Read secrets
var connectionString = ReadSecret("/run/secrets/db-connection-string");
var token = ReadSecret("/run/secrets/token");
var apiUri = ReadSecret("/run/secrets/api-uri");

// Inject ScraperSettings using DI
builder.Services.AddSingleton(new ScraperSettings
{
    Token = token,
    ApiUri = apiUri
});

// Hangfire setup
builder.Services.AddHangfire(cfg =>
    cfg.UsePostgreSqlStorage(connectionString));
builder.Services.AddHangfireServer();

// Register application services
builder.Services.AddSingleton<JobRunner>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllUsersAuthorizationFilter() }
});

app.MapControllers();

app.Run();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Scraper API started successfully.");
