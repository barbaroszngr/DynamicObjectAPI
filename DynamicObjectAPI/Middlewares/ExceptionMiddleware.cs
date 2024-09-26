using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;
using DynamicObjectAPI.Common.Exceptions;
using System.Text.Json;


namespace DynamicObjectAPI.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (ValidationException ve)
            {
                await HandleExceptionAsync(httpContext, ve.Message, HttpStatusCode.BadRequest);
            }
            catch (NotFoundException nfe)
            {
                await HandleExceptionAsync(httpContext, nfe.Message, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, "Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, string message, HttpStatusCode statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var result = JsonSerializer.Serialize(new
            {
                StatusCode = context.Response.StatusCode,
                Message = message
            });

            return context.Response.WriteAsync(result);
        }
    }
}