using ElasoftCommunityManagementSystem.Exceptions;
using System.Net;
using System.Text.Json;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ElasoftCommunityManagementSystem.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
            _logger = Log.ForContext<ExceptionHandlingMiddleware>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    TraceId = context.TraceIdentifier,
                    Message = error.Message
                };

                // Geliştirme ortamında ise daha detaylı hata bilgisi ekle
                if (_env.IsDevelopment())
                {
                    errorResponse.DeveloperMessage = error.ToString();
                    errorResponse.StackTrace = error.StackTrace;
                }

                switch (error)
                {
                    case UnauthorizedAccessException e:
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        errorResponse.Message = "Unauthorized access";
                        _logger.Warning("Unauthorized access: {Message}, TraceId: {TraceId}", e.Message, context.TraceIdentifier);
                        break;
                    case BusinessException e:
                        response.StatusCode = (int)e.StatusCode;
                        errorResponse.Message = e.Message;
                        _logger.Warning("{ExceptionType}: {Message}, TraceId: {TraceId}", 
                            e.GetType().Name, e.Message, context.TraceIdentifier);
                        break;
                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        errorResponse.Message = "An unexpected error occurred.";
                        _logger.Error(error, "Unexpected error occurred: {Message}, TraceId: {TraceId}", 
                            error.Message, context.TraceIdentifier);
                        break;
                }

                var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });
                await response.WriteAsync(result);
            }
        }
    }

    public class ErrorResponse
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public required string TraceId { get; set; }
        public string? DeveloperMessage { get; set; }
        public string? StackTrace { get; set; }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}