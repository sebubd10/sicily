using System.Net;
using System.Text.Json;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BasicCommerce.Infrastructure.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next,
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

            DbUpdateConcurrencyException dce => (
                HttpStatusCode.Conflict,
                ApiResponse<object>.Fail(
                    _env.IsDevelopment()
                        ? $"{dce.Message} Entities involved: " +
                          string.Join(", ", dce.Entries.Select(entry =>
                              $"{entry.Metadata.ClrType.Name}[{DescribeKey(entry)}] (state: {entry.State})"))
                        : "This record was changed by another operation. Please retry.")),

            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail(
                    _env.IsDevelopment()
                        ? $"{exception.GetType().Name}: {exception.Message}" +
                          (exception.InnerException is not null
                              ? $" | Inner: {exception.InnerException.Message}"
                              : string.Empty)
                        : "An unexpected error occurred."))
        };

        if (statusCode is HttpStatusCode.InternalServerError or HttpStatusCode.Conflict)
            _logger.LogError(exception, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }

    private static string DescribeKey(EntityEntry entry)
    {
        var keyProps = entry.Properties.Where(p => p.Metadata.IsPrimaryKey());
        return string.Join(",", keyProps.Select(p => $"{p.Metadata.Name}={p.CurrentValue}"));
    }
}
