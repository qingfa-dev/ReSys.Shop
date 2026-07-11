using FluentValidation;
using FluentValidation.Results;

using MediatR;

namespace Shared.Application.Mediators.Behaviours.Validation;

/// <summary>
/// Pipeline behavior that validates requests using FluentValidation validators before processing.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response (must inherit from Result).</typeparam>
public sealed partial class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResultRecord
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            Loggers.NoValidators(logger, typeof(TRequest).Name);

            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        ValidationResult[] results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        if (logger.IsEnabled(LogLevel.Warning))
        {
            Loggers.ValidationFailed(
                logger,
                typeof(TRequest).Name,
                string.Join(", ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));
        }

        var validationFailures = failures
            .Select(f => Error.Validation(
                f.ErrorCode ?? f.PropertyName,
                f.ErrorMessage,
                ("fields", f.PropertyName)))
            .ToList();

        return (TResponse)(dynamic)Result.Validation(errors: validationFailures);
    }
}
