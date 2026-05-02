using System.Net;
using System.Text.Json;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BasicCommerce.Infrastructure.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.UnprocessableEntity,
                ApiResponse<object>.Fail(
                    "Validation failed.",
                    ve.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))),

            NotFoundException nfe => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(nfe.Message)),

            UnauthorizedException ue => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail(ue.Message)),

            DomainException de => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(de.Message)),

            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("An unexpected error occurred."))
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }
}
