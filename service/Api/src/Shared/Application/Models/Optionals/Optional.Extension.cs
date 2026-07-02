namespace Shared.Application.Models.Optionals;

public static class OptionalExtensions
{
    #region Apply
    // Execute: Set value on target via setter when optional has value — returns success flag
    public static bool Apply<T>(
        this Optional<T> optional,
        Action<T> setter)
    {
        // Guard: Setter must not be null
        ArgumentNullException.ThrowIfNull(setter);

        // Skip: Return false when optional is empty — no value to apply
        if (!optional.HasValue)
            return false;

        setter(optional.Value!);
        return true;
    }

    // Execute: Set value only when current differs — prevents redundant property-change side effects
    public static bool ApplyIfChanged<T>(
        this Optional<T> optional,
        T current,
        Action<T> setter)
    {
        // Guard: Setter must not be null
        ArgumentNullException.ThrowIfNull(setter);

        // Skip: Return false when optional is empty
        if (!optional.HasValue)
            return false;

        // Skip: Return false when current value matches — no change needed
        if (EqualityComparer<T>.Default.Equals(current, optional.Value))
            return false;

        setter(optional.Value!);
        return true;
    }

    // Check: Set value only when predicate passes — conditional update based on business rule
    public static bool ApplyIf<T>(
        this Optional<T> optional,
        Func<T, bool> predicate,
        Action<T> setter)
    {
        // Guard: Predicate and setter must not be null
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(setter);

        // Skip: Return false when optional is empty
        if (!optional.HasValue)
            return false;

        // Skip: Return false when predicate fails — value does not meet condition
        if (!predicate(optional.Value!))
            return false;

        setter(optional.Value!);
        return true;
    }
    #endregion

    #region ApplyValidated
    // Validate: Apply value only after validator passes — returns Result for caller to inspect
    public static Result ApplyValidated<T>(
        this Optional<T> optional,
        Func<T, Result> validator,
        Action<T> setter)
    {
        // Guard: Validator and setter must not be null
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(setter);

        // Skip: Return Ok when optional is empty — nothing to validate, no side effect
        if (!optional.HasValue)
            return Result.Ok();

        // Validate: Run business validator on contained value before applying
        Result validation = validator(optional.Value!);
        if (validation.IsFailure)
            return validation;

        setter(optional.Value!);

        return Result.Ok();
    }
    #endregion

    #region Result / Error Integration
    // Convert: Optional to Result — Some becomes Ok, None becomes BadRequest with provided error
    public static Result<T> ToResult<T>(
        this Optional<T> optional,
        Error error)
    {
        // Skip: Return BadRequest Result when optional is empty — missing value mapped to error
        if (!optional.HasValue)
            return Result<T>.BadRequest(errors: [error]);

        return Result<T>.Ok(optional.Value!);
    }

    // Convert: Optional to Result with lazy error factory — factory invoked only when empty
    public static Result<T> ToResult<T>(
        this Optional<T> optional,
        Func<Error> errorFactory)
    {
        // Guard: Error factory must not be null
        ArgumentNullException.ThrowIfNull(errorFactory);

        // Skip: Return BadRequest Result with lazily created error when empty
        if (!optional.HasValue)
            return Result<T>.BadRequest(errors: [errorFactory()]);

        return Result<T>.Ok(optional.Value!);
    }
    #endregion

    #region Match
    // Branch: Exhaustive pattern matching — some branch for value, none branch for empty
    //         Enables functional-style transformation without nullable/exception ambiguity
    public static TResult Match<T, TResult>(
        this Optional<T> optional,
        Func<T, TResult> some,
        Func<TResult> none)
    {
        // Guard: Both branches must be provided — exhaustive matching contract
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(none);

        return optional.HasValue
            ? some(optional.Value!)
            : none();
    }
    #endregion

    #region SelectMany
    // Combine: Flatten nested Optionals for LINQ query comprehension syntax
    //          Enables: from x in opt1 from y in opt2 select x + y
    public static Optional<TResult> SelectMany<TSource, TCollection, TResult>(
        this Optional<TSource> source,
        Func<TSource, Optional<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        // Guard: Collection selector and result selector must not be null
        ArgumentNullException.ThrowIfNull(collectionSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        // Skip: Short-circuit when source optional is empty
        if (!source.HasValue)
            return Optional<TResult>.None;

        Optional<TCollection> collection = collectionSelector(source.Value!);

        // Skip: Short-circuit when collection optional is empty
        if (!collection.HasValue)
            return Optional<TResult>.None;

        // Compute: Apply result selector only when both optionals have values
        return Optional<TResult>.Some(resultSelector(source.Value!, collection.Value!));
    }
    #endregion
}
