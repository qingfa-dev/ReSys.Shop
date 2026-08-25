=== Vertical Slice Architecture

The backend implements *Vertical Slice Architecture* (VSA), grouping code by business feature rather than technical layer. Rather than spreading a feature across separated controllers, services, and repositories, VSA co-locates the request model, route definition, handler logic, validation rules, and response DTO within a single feature directory. This layout accelerates development velocity and simplifies maintenance by keeping all context for a feature in one place.

==== Feature Co-Location and Endpoint Pipeline

Features use *Carter Minimal APIs* for route discovery and *MediatR* for command/query dispatch. Each of the eight bounded contexts defines an #emph[ICarterModule] to register its HTTP endpoints. Endpoints remain intentionally thin: they parse incoming HTTP requests, construct a MediatR command or query, dispatch it via #emph[ISender], and map the returned #emph("Result<T>") directly to HTTP responses.

All business rules, database queries, and domain invariants reside exclusively inside feature handlers, making them fully testable isolated from HTTP dependencies. *FluentValidation* classes are co-located with their target requests, validating data models before handler execution via MediatR pipeline behaviors.

==== Cross-Cutting MediatR Pipeline

Every dispatched command and query traverses a central MediatR pipeline that applies cross-cutting concerns uniformly across all features:

- *Validation Behavior:* Intercepts requests, executes co-located FluentValidation rules, and short-circuits with structured validation failures on error.
- *Logging Behavior:* Captures request execution context, timings, and parameters for diagnostic tracing.
- *Exception-Mapping Behavior:* Catches unhandled domain exceptions and translates them into predictable API responses.
- *Transactional Behavior:* Enforces feature-level transaction boundaries, wrapping multi-row operations within an explicit database unit of work.

==== Inter-Module Boundaries

The architecture enforces a strict isolation rule: *no bounded context may directly import another context's namespace*. Inter-module integration relies exclusively on MediatR's #emph[ISender] interface via in-process query dispatch or event notifications. This maintains microservice-like logical isolation and clean domain boundaries while keeping the deployment simplicity of a monolithic executable.