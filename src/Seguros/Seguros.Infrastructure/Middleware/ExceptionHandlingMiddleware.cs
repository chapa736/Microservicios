using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Seguros.Core.Common;
using Seguros.Core.Exceptions;

namespace Seguros.Infrastructure.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> _logger)
        {
            _next = next;
            this._logger = _logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción no controlada");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new BaseResponse<object>
            {
                Success = false
            };

            switch (exception)
            {
                case NotFoundException notFoundEx:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = notFoundEx.Message;
                    break;
                    
                case ValidationException validationEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = validationEx.Message;
                    response.Errors = validationEx.Errors;
                    break;
                    
                case DuplicateException duplicateEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Message = duplicateEx.Message;
                    break;
                    
                case BusinessException businessEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = businessEx.Message;
                    break;
                    
                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "Acceso no autorizado";
                    break;
                    
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "Error interno del servidor";
                    response.Errors.Add(exception.Message);
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
