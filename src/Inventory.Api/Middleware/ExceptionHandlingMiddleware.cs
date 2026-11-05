using FluentValidation;
using Inventory.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
namespace Inventory.Api.Middleware;
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problem = exception switch
        {
            ValidationException validation => BuildProblem(StatusCodes.Status400BadRequest, "Validation failed", validation.Errors.Select(error => error.ErrorMessage)),
            NotFoundException notFound => BuildProblem(StatusCodes.Status404NotFound, "Resource not found", [notFound.Message]),
            ConflictException conflict => BuildProblem(StatusCodes.Status409Conflict, "Conflict", [conflict.Message]),
            _ => BuildUnexpectedProblem(exception)
        };
        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
    private ProblemDetails BuildUnexpectedProblem(Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception while processing request");
        return BuildProblem(StatusCodes.Status500InternalServerError, "Unexpected error", ["An unexpected error occurred."]);
    }
    private static ProblemDetails BuildProblem(int status, string title, IEnumerable<string> errors)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = $"https://httpstatuses.com/{status}"
        };
        problem.Extensions["errors"] = errors.ToArray();
        return problem;
    }
}
