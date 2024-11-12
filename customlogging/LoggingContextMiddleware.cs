using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace customlogging
{
    public class LogContextMiddleware
    {
        private readonly RequestDelegate _next;

        public LogContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (LogContext.PushProperty("Log_CorrelationId", context.GetCorrelationId()))
            using (LogContext.PushProperty("Log_RequestMethod", context.GetOperationType()))
            using (LogContext.PushProperty("Log_OperationName", context.GetActionName()))
            {
                await _next(context);
            }
        }
    }
}
