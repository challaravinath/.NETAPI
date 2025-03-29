namespace ProductApi.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log the request
            await LogRequest(context);

            // Capture the response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                // Continue down the middleware pipeline
                await _next(context);

                // Log the response
                await LogResponse(context);
            }
            finally
            {
                // Copy the response to the original stream and restore it
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();

            // Read and log request body for specific content types only
            if (context.Request.ContentType?.Contains("application/json") == true &&
                context.Request.ContentLength > 0)
            {
                var buffer = new byte[context.Request.ContentLength.Value];
                await context.Request.Body.ReadAsync(buffer);
                var requestBody = System.Text.Encoding.UTF8.GetString(buffer);

                _logger.LogInformation(
                    "HTTP {RequestMethod} {RequestPath} Request Body: {RequestBody}",
                    context.Request.Method, context.Request.Path, requestBody);

                // Reset the request body stream position
                context.Request.Body.Position = 0;
            }
            else
            {
                _logger.LogInformation(
                    "HTTP {RequestMethod} {RequestPath}",
                    context.Request.Method, context.Request.Path);
            }
        }

        private async Task LogResponse(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            if (!string.IsNullOrEmpty(responseBodyText) && responseBodyText.Length < 4000)
            {
                _logger.LogInformation(
                    "HTTP {RequestMethod} {RequestPath} responded {StatusCode} Response Body: {ResponseBody}",
                    context.Request.Method, context.Request.Path, context.Response.StatusCode, responseBodyText);
            }
            else
            {
                _logger.LogInformation(
                    "HTTP {RequestMethod} {RequestPath} responded {StatusCode} [Response body too large to log]",
                    context.Request.Method, context.Request.Path, context.Response.StatusCode);
            }
        }
    }

    // Extension method for cleaner registration
    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
