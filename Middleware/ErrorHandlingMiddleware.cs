using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace TE_Project.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ErrorHandlingMiddleware(
            RequestDelegate next, 
            ILogger<ErrorHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title) = GetStatusCodeAndTitle(exception);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = _environment.IsDevelopment() ? exception.ToString() : exception.Message,
                Instance = context.Request.Path,
                Type = $"https://httpstatuses.com/{statusCode}"
            };

            if (_environment.IsDevelopment())
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
                problemDetails.Extensions["source"] = exception.Source;

                // Add inner exception details if available
                if (exception.InnerException != null)
                {
                    problemDetails.Extensions["innerException"] = new Dictionary<string, string>
                    {
                        ["message"] = exception.InnerException.Message,
                        ["type"] = exception.InnerException.GetType().Name
                    };
                }
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problemDetails);
        }

        private static (int StatusCode, string Title) GetStatusCodeAndTitle(Exception exception)
        {
            return exception switch
            {
                ArgumentException or InvalidOperationException or FluentValidation.ValidationException 
                    => (StatusCode: (int)HttpStatusCode.BadRequest, Title: "Bad Request"),
                KeyNotFoundException 
                    => (StatusCode: (int)HttpStatusCode.NotFound, Title: "Resource Not Found"),
                UnauthorizedAccessException 
                    => (StatusCode: (int)HttpStatusCode.Unauthorized, Title: "Unauthorized"),
                System.Security.Authentication.AuthenticationException 
                    => (StatusCode: (int)HttpStatusCode.Forbidden, Title: "Forbidden"),
                NotImplementedException 
                    => (StatusCode: (int)HttpStatusCode.NotImplemented, Title: "Not Implemented"),
                _ => (StatusCode: (int)HttpStatusCode.InternalServerError, Title: "Internal Server Error")
            };
        }
    }
}