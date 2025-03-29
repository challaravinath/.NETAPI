
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Repositories.Interfaces;
using ProductApi.Repositories;
using ProductApi.Data;
using ProductApi.Middleware;

namespace ProductApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register repository as a scoped service
            builder.Services.AddScoped<IProductRepository, ProductRepository>();

            // Add response caching
            builder.Services.AddResponseCaching();

            var app = builder.Build();

            // Seed database
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                await DbInitializer.Initialize(context); // Now it can find DbInitializer
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            //app.UseMiddleware<PaginationHeaderMiddleware>();
            app.UsePaginationHeaders();

            app.MapControllers();

            app.Run();
        }
    }
}
