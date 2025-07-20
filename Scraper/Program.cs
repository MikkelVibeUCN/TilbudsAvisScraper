using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scraper;

var builder = WebApplication.CreateBuilder(args);

// Load settings from environment or appsettings.json
builder.Services.Configure<ScraperSettings>(builder.Configuration.GetSection("ScraperSettings"));

// PostgreSQL connection string for Hangfire
var connectionString = builder.Configuration.GetConnectionString("Default");

// Add Hangfire services using PostgreSQL
builder.Services.AddHangfire(cfg =>
    cfg.UsePostgreSqlStorage(connectionString));
builder.Services.AddHangfireServer();

// Register app services
builder.Services.AddSingleton<JobRunner>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware: Swagger and Hangfire dashboard
app.UseSwagger();
app.UseSwaggerUI();
app.UseHangfireDashboard("/hangfire");

// Map API routes
app.MapControllers();

app.Run();
