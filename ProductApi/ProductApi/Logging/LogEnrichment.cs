using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.AspNetCore;  // This should resolve IDiagnosticContext

namespace ProductApi.Logging
{
    public static class LogEnrichment
    {
        public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress);
            diagnosticContext.Set("RequestHost", httpContext.Request.Host);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);

            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
                // Add claims if needed
            }
        }
    }
}