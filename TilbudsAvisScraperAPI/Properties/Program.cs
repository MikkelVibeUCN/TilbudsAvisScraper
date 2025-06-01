using DAL.Data.DAO;
using DAL.Data.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;
using TIlbudsAvisScraperAPI.Services;

namespace TIlbudsAvisScraperAPI.Properties
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string connectionString;

            var secretPath = "/run/secrets/connection_string";
            if (File.Exists(secretPath))
            {
                connectionString = File.ReadAllText(secretPath);
            }
            else
            {
                connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING");
            }

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddScoped<IAvisDAO>(provider => new AvisDAO(new ProductDAO(new NutritionInfoDAO(connectionString), new PriceDAO(connectionString), connectionString), connectionString));
            builder.Services.AddScoped<IProductDAO>(provider => new ProductDAO(new NutritionInfoDAO(connectionString), new PriceDAO(connectionString), connectionString));
            builder.Services.AddScoped<APIUserService>(provider => new APIUserService(new APIUserDAO(connectionString)));
            builder.Services.AddScoped<ProductService>(provider =>
                new ProductService(
                    provider.GetRequiredService<IProductDAO>()
                ));
            builder.Services.AddScoped<AvisService>(provider => new AvisService(provider.GetRequiredService<IAvisDAO>()));
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Enable forwarded headers for NGINX
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear(); 
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseForwardedHeaders(); // Ensure this is before other middleware

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
