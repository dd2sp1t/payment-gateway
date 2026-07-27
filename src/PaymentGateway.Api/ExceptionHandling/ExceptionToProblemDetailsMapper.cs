using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Domain.Exceptions;

namespace PaymentGateway.Api.ExceptionHandling;

internal static class ExceptionToProblemDetailsMapper
{
    public static ProblemDetails Map(HttpContext httpContext, Exception exception)
    {
        return exception switch
        {
            ProviderPaymentMismatchException providerPaymentMismatchException =>
                Create(
                    httpContext,
                    StatusCodes.Status409Conflict,
                    "Provider payment mismatch",
                    providerPaymentMismatchException.Message),

            DomainException domainException =>
                Create(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "Domain validation error",
                    domainException.Message),

            RequestValidationException requestValidationException =>
                Create(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "Request validation error",
                    requestValidationException.Message),

            NotFoundException notFoundException =>
                Create(
                    httpContext,
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    notFoundException.Message),

            DuplicateResourceException duplicateResourceException =>
                Create(
                    httpContext,
                    StatusCodes.Status409Conflict,
                    "Duplicate resource",
                    duplicateResourceException.Message),

            PersistenceException persistenceException =>
                Create(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    "Persistence error",
                    persistenceException.Message),

            _ =>
                Create(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    "Internal server error",
                    "Unexpected error occurred.")
        };
    }

    private static ProblemDetails Create(HttpContext httpContext, int status, string title, string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
    }
}