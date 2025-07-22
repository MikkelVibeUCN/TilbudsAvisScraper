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

builder.Services.Configure<ScraperSettings>(options =>
{
    options.Token = token;
    options.ApiUri = apiUri;
});

// Hangfire setup
builder.Services.AddHangfire(cfg =>
    cfg.UsePostgreSqlStorage(connectionString));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1; // Only one job runs at a time across all types
});

// Register application services
builder.Services.AddTransient<JobRunner>();
builder.Services.AddTransient<ScraperBootstrapper>();


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

app.MapControllers(); // <== THIS is what makes [ApiController] routes work


app.Run();