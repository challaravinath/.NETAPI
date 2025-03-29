namespace ProductApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            // FIX: Create an object with a consistent shape
            object response;

            if (_env.IsDevelopment())
            {
                // Full details for development
                response = new
                {
                    status = context.Response.StatusCode,
                    message = exception.Message,
                    stackTrace = exception.StackTrace
                };
            }
            else
            {
                // Limited info for production
                response = new
                {
                    status = context.Response.StatusCode,
                    message = "An error occurred. Please try again later.",
                    stackTrace = (string)null // Include the property but set to null
                };
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }

    // Extension method for middleware registration
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
