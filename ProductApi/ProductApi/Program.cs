
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Repositories.Interfaces;
using ProductApi.Repositories;
using ProductApi.Middleware;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProductApi.Services;
using Azure;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using ProductApi.Logging;

namespace ProductApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("logs", "log-.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog(); // Tell ASP.NET Core to use Serilog


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
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>("database")
                .AddCheck("self", () => HealthCheckResult.Healthy());
            builder.Services.AddHostedService<PerformanceMetricsService>();
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
            // app.UseSerilogRequestLogging();
            // Enhanced version with custom properties
            app.UseSerilogRequestLogging(options => {
                options.EnrichDiagnosticContext = LogEnrichment.EnrichFromRequest;
            });
            app.UseMiddleware<PaginationHeaderMiddleware>();
            app.UsePaginationHeaders();
            // Add exception handling first to catch all exceptions
            app.UseCustomExceptionHandler();
            // Map health checks endpoint
            app.MapHealthChecks("/health");
            app.UseRequestResponseLogging();


            // Map health checks endpoint
            app.MapHealthChecks("/health");

            // Add detailed request/response logging (optional - can be verbose)
            // app.UseRequestResponseLogging();

            // Map health checks endpoint
            app.MapControllers();

            app.Run();
        }
    }
}
