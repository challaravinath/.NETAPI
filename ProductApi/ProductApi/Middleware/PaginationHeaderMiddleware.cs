using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ProductApi.Middleware
{
    public class PaginationHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PaginationHeaderMiddleware> _logger;
        private readonly bool _isDevelopment;

        public PaginationHeaderMiddleware(
            RequestDelegate next,
            ILogger<PaginationHeaderMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _isDevelopment = env.IsDevelopment();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context); // call next middleware

            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            // Check if we have a JSON response
            if (context.Response.ContentType?.Contains("application/json") == true)
            {
                try
                {
                    // Better detection method - try to deserialize and check for required properties
                    var json = JsonSerializer.Deserialize<JsonElement>(responseBody);

                    // Check if this is a paged response by looking for required properties
                    bool isPagedResponse =
                        json.TryGetProperty("totalCount", out _) &&
                        json.TryGetProperty("pageNumber", out _) &&
                        json.TryGetProperty("pageSize", out _);

                    if (isPagedResponse)
                    {
                        _logger.LogDebug("Detected paginated response, adding pagination headers");

                        if (json.TryGetProperty("totalCount", out var totalCount))
                            context.Response.Headers.Append("X-Pagination-TotalCount", totalCount.ToString());

                        if (json.TryGetProperty("pageNumber", out var pageNumber))
                            context.Response.Headers.Append("X-Pagination-PageNumber", pageNumber.ToString());

                        if (json.TryGetProperty("pageSize", out var pageSize))
                            context.Response.Headers.Append("X-Pagination-PageSize", pageSize.ToString());

                        if (json.TryGetProperty("totalPages", out var totalPages))
                            context.Response.Headers.Append("X-Pagination-TotalPages", totalPages.ToString());

                        // Fix the property names
                        if (json.TryGetProperty("hasPreviousPage", out var hasPrevPage))
                            context.Response.Headers.Append("X-Pagination-HasPrevPage", hasPrevPage.ToString());

                        if (json.TryGetProperty("hasNextPage", out var hasNextPage))
                            context.Response.Headers.Append("X-Pagination-HasNextPage", hasNextPage.ToString());
                    }
                }
                catch (JsonException ex)
                {
                    
                    _logger.LogWarning(ex, "Error processing potential paginated response");

                    // In development, you might want to add a debug header
                    if (_isDevelopment)
                    {
                        context.Response.Headers.Append("X-Pagination-Error",
                            "Failed to process pagination: " + ex.Message);
                    }
                }
            }

            // Reset the memory stream and copy it to the original response body
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
        }
    }

    // Extension method to make it easier to add the middleware
    public static class PaginationHeaderMiddlewareExtensions
    {
        public static IApplicationBuilder UsePaginationHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PaginationHeaderMiddleware>();
        }
    }
}