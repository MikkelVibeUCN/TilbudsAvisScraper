using APIIntegrationLibrary.Client;
using APIIntegrationLibrary.Interfaces;
using System;
using TilbudsAvisWeb.Services;

namespace TilbudsAvisWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            string uri = "http://api.tilbudsfinder.dk/v1/";
            //string uri = "https://localhost:7133/v1/";

            // Add services to the container.
            builder.Services.AddScoped<IProductAPIRestClient>(provider => new ProductAPIRestClient(uri));

            builder.Services.AddScoped<ProductService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseForwardedHeaders();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
