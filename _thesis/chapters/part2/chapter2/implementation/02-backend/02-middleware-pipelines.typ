==== Request Pipeline & Middleware

The application utilizes *MediatR Behaviors* to implement cross-cutting concerns such as logging, validation, and error handling within the request pipeline. This Middleware pattern ensures that the core business logic handlers remain clean and focused on their specific task.

*Validation Pipeline:*
Before a handler executes, the `ValidationBehavior` intercepts the request, runs all defined `FluentValidation` rules, and returns a structured error response if validation fails.

```cs
public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(...) {
        if (validator == null) return await next();

        var result = await validator.ValidateAsync(request);
        if (result.IsValid) return await next();

        return (dynamic)result.Errors.ConvertAll(e => Error.Validation(...));
    }
}
```

#figure(
  placement: none,
  image("../../../../../images/diagrams/02-system-architecture/sys-05-request-pipeline.png", width: 60%),
  caption: [MediatR Request Pipeline: Visual tracking of the "Onion Architecture" flow where behaviors wrap the core handler.],
)
