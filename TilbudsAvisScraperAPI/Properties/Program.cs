using DAL.Data.DAO;
using DAL.Data.Interfaces;
using TIlbudsAvisScraperAPI.Services;

namespace TIlbudsAvisScraperAPI.Properties
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING");

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddScoped<IAvisDAO>(provider => new AvisDAO(new ProductDAO(new NutritionInfoDAO(connectionString), new PriceDAO(connectionString), connectionString), connectionString));
            builder.Services.AddScoped<IProductDAO>(provider => new ProductDAO(new NutritionInfoDAO(connectionString), new PriceDAO(connectionString), connectionString));
            builder.Services.AddScoped<APIUserService>(provider => new APIUserService(new APIUserDAO(connectionString)));
            builder.Services.AddScoped<ProductService>(provider =>
                new ProductService(
                    provider.GetRequiredService<IProductDAO>()
                ));
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
