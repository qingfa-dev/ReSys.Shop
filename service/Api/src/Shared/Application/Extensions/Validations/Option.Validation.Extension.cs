using FluentValidation;
using FluentValidation.Results;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Shared.Application.Extensions.Validations;

/// <summary>
/// Extension methods for <see cref="OptionsBuilder{TOptions}"/> to integrate FluentValidation with the Options Pattern.
/// </summary>
public static class OptionsBuilderExtensions
{
    public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(this OptionsBuilder<TOptions> builder)
        where TOptions : class
    {
        // Register: FluentValidateOptions<TOptions> as singleton IValidateOptions — resolves validator per call
        builder.Services.AddSingleton<IValidateOptions<TOptions>>(
            serviceProvider => new FluentValidateOptions<TOptions>(
                serviceProvider,
                builder.Name));

        return builder;
    }
}
/// <summary>
/// Generic <see cref="IValidateOptions{T}"/> implementation that resolves a FluentValidation <see cref="IValidator{T}"/>
/// from the DI container and runs validation at startup. Supports named options through the <c>name</c> parameter.
/// </summary>
public sealed class FluentValidateOptions<TOptions>(
    IServiceProvider serviceProvider,
    string? name) : IValidateOptions<TOptions>
    where TOptions : class
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly string? _name = name;

    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        // Skip: Named-options mismatch — delegate to other registered validators
        if (_name is not null && !string.Equals(_name, name, StringComparison.Ordinal))
        {
            return ValidateOptionsResult.Skip;
        }

        // Guard: Null options at validation time signals a registration defect
        ArgumentNullException.ThrowIfNull(options);

        // Acquire: Scoped IServiceScope for IValidator resolution — ensures correct lifetime
        using IServiceScope scope = _serviceProvider.CreateScope();

        // Resolve: IValidator<TOptions> from DI — registered by caller via AddSingleton
        IValidator<TOptions> validator = scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>();

        // Validate: Run FluentValidation rules against materialized options
        ValidationResult result = validator.Validate(options);

        if (result.IsValid)
            return ValidateOptionsResult.Success;

        // Format: Human-readable failure messages with fully qualified property paths
        string typeName = options.GetType().Name;
        List<string> errors = [];

        foreach (ValidationFailure failure in result.Errors)
        {
            errors.Add($"Validation failed for {typeName}.{failure.PropertyName} " +
                       $"with the error: {failure.ErrorMessage}");
        }

        return ValidateOptionsResult.Fail(errors);
    }
}
