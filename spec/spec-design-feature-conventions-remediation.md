---
title: Feature Command/Query/Request/Response Convention Standardization
version: 1.0
date_created: 2026-07-16
owner: Platform Team
tags: design, convention, feature-slice, remediation
---

# Introduction

Standardize every feature Command/Query/Request/Response across the 8 business modules to follow a uniform pattern. Eliminate 4 categories of deviation (inline-field commands, unbased responses, unbased requests, manual-construction handlers) so the codebase is consistent, machine-auditable, and maintainable.

## 1. Purpose & Scope

**Purpose**: Establish an unambiguous, enforceable set of conventions for every feature slice in the `Module/` assembly. All 8 business modules (Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping) are in scope. The Shared assembly (`Shared/Application/Models/`) is out of scope except as the source of base mediator interfaces (`ICommand<T>`, `IQuery<T>`, `IPagedQuery<T>`).

**Audience**: Agents and developers implementing new features or remediating existing ones.

**Assumptions**: The existing 3-level model hierarchy (`Parameters` → `Request` → `Response`) and Mapster-style mapping pattern (`MapToDetail<T>()`) are correct and should be extended, not replaced.

## 2. Definitions

| Term | Definition |
|---|---|
| Feature slice | A vertical-slice action within a `static partial class` — e.g., `CreateProduct`, `ApproveOrder` — containing Handler, Request, Response, Endpoint, Validator files. |
| Command | A `sealed record` implementing `ICommand<TResponse>` or `ICommand` (void). Must wrap a Request record or take only a `Guid Id`. |
| Query | A `sealed record` implementing `IQuery<TResponse>` or `IPagedQuery<TResponse>`. Must wrap a Parameters record or take only a `Guid Id`/`string Slug`. |
| Request | A `record` (or `class` in Profile) inheriting from a `{Entity}Request` base defined in the feature group's `Shared/Models/`. |
| Response | A `sealed record` inheriting from a `{Entity}DetailResponse` or `{Entity}ListItemResponse` base defined in the feature group's `Shared/Models/`. |
| Parameters | An `abstract record` defining the shape properties shared between Request and Response. |
| Mapping method | A `static` extension method in `{Entity}.Mapping.Model.cs` of the form `T MapToDetail<T>(this Entity)` or `T MapToListItem<T>(this Entity)` where `T : BaseResponse, new()`. |
| Module-internal base | A base class defined in `Module/{Domain}/Features/{Area}/{Feature}/Shared/Models/` — not in the `Shared/` assembly. This is the **expected** location; bases need NOT live in the Shared assembly. |

## 3. Requirements, Constraints & Guidelines

### RQ-001: Command/Query structure
Every Command/Query record must follow exactly one of these patterns:

| Pattern | Example | When |
|---|---|---|
| `record Command(Request Request) : ICommand<Response>` | `CreateProduct.Command` | Create/update with body |
| `record Command(Guid Id) : ICommand<Response>` | `ApproveOrder.Command` | ID-only action with typed response |
| `record Command(Guid Id) : ICommand` | `DeleteProduct.Command` | ID-only void action |
| `record Command(Guid Id, Request Request) : ICommand<Response>` | `CancelOrderAdmin.Command` | ID + body |
| `record Query(Guid Id) : IQuery<Response>` | `GetProductById.Query` | Single-entity fetch |
| `record Query(string Slug) : IQuery<Response>` | `GetProductDetail.Query` | Slug-based fetch |
| `record Query(Parameters Parameters) : IPagedQuery<Response>` | `GetPagedOrders.Query` | Paged list fetch |

**No other property combination is permitted.** A Command/Query must NOT inline domain fields directly.

### RQ-002: Request must inherit from module-internal base
Every Request record must inherit from a `{Entity}Request` base defined in the feature group's `Shared/Models/`:
```csharp
public record Request : ProductRequest;
```
Exception: features with no request body (ID-only commands) correctly have no Request file.

### RQ-003: Response must inherit from module-internal base
Every Response record must inherit from a `{Entity}DetailResponse` or `{Entity}ListItemResponse` base:
```csharp
public record Response : ProductDetailResponse;
```
Standalone `record Response` with no base is forbidden. Inheritance from a service-internal result/domain type is also forbidden.

### RQ-004: Handler must use mapping method for Response construction
Every Handler must construct its Response via a `MapToDetail<T>()`, `MapToListItem<T>()`, or equivalent mapping extension method:
```csharp
return entity.MapToDetail<Response>();           // detail response
return items.MapToList<Response>();              // list response
return Result<Response>.Created(                 // create with 201
    entity.MapToDetail<Response>(),
    ProductResult.Success.Created(entity.Id));
```
Manual construction via `new Response { ... }` or `new Response(...)` is forbidden, unless the feature genuinely has no domain entity mapping path (e.g., an image serving endpoint returning raw bytes).

### RQ-005: Command with IFormFile
File-upload commands (IFormFile) must wrap the file in a Request record:
```csharp
public record Request : ProductRequest
{
    public required IFormFile Image { get; init; }
}
```
Inline `Command(IFormFile Image)` is forbidden.

### RQ-006: Feature file naming
Every feature action must have its files split as follows (consistent with the existing convention):

| File | Content |
|---|---|
| `{Action}.cs` | `static partial class {Action}` containing `Command`/`Query` record + `Handler` class |
| `{Action}.Request.cs` | Nested `record Request : {Entity}Request` |
| `{Action}.Response.cs` | Nested `record Response : {Entity}DetailResponse` (or `ListItemResponse`) |
| `{Action}.Endpoint.cs` | Nested `class Endpoint : ICarterModule` |
| `{Action}.Validator.cs` | Nested `class Validator : AbstractValidator<Request>` (optional) |

### GUD-001: Status code for creation endpoints
Handlers for create operations must use `Result<T>.Created(...)` (HTTP 201), not the implicit `T → Result<T>` operator (HTTP 200). The `Created` factory is defined in `Shared/Application/Models/Results/ValueResult.Method.cs`.

### GUD-002: Error return style
Handlers should return errors via `result.Errors` (implicit `List<Error> → Result<T>` operator), not explicit casts `(Result<Response>)result.Errors`.

### GUD-003: Module-internal Shared models are correct
The `{Entity}Parameters` / `{Entity}Request` / `{Entity}DetailResponse` base types should remain in `Module/{Domain}/Features/{Area}/{Feature}/Shared/Models/`. They are NOT required to move to the `Shared/` assembly. The convention is that each feature group owns its model hierarchy locally.

## 4. Interfaces & Data Contracts

### 4.1 Mediator interfaces (from Shared assembly)

```csharp
// — Shared.Application.Mediators.Commands —
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
public interface ICommand : IRequest<Result>;

// — Shared.Application.Mediators.Queries —
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
public interface IPagedQuery<TResponse> : IRequest<PagedResult<TResponse>>;
```

### 4.2 Feature slice contract

Every feature follows this structural contract:

```
Module/{Domain}/Features/{Area}/{Feature}/
├── {Action}.cs                  # Command/Query + Handler
├── {Action}.Request.cs          # Request : {Entity}Request
├── {Action}.Response.cs         # Response : {Entity}DetailResponse
├── {Action}.Endpoint.cs         # Carter endpoint
├── {Action}.Validator.cs        # FluentValidation (optional)
└── Shared/
    ├── Models/
    │   ├── {Entity}.Model.Parameters.cs     # abstract record {Entity}Parameters
    │   ├── {Entity}.Model.Request.cs        # record {Entity}Request : {Entity}Parameters
    │   └── {Entity}.Model.Response.cs       # record {Entity}DetailResponse : {Entity}Parameters
    ├── Mappings/
    │   ├── {Entity}.Mapping.Domain.cs       # Request → domain
    │   └── {Entity}.Mapping.Model.cs        # Domain → Response DTO
    └── Validators/
        └── {Entity}.Validator.cs
```

### 4.3 Model hierarchy contract

```csharp
// Parameters: shared shape, abstract, no identity
public abstract record ProductParameters
{
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    // ...
}

// Request: inherits Parameters, no additional fields
public record ProductRequest : ProductParameters;

// Detail Response: inherits Parameters, adds Id + audit
public record ProductDetailResponse : ProductParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    // ...
}

// ListItem Response: same as Detail but may differ
public record ProductListItemResponse : ProductParameters
{
    public Guid Id { get; init; }
    public int VariantsCount { get; init; }
    // ...
}
```

### 4.4 Mapping method contract

```csharp
public static T MapToDetail<T>(this Product entity) where T : ProductDetailResponse, new()
{
    return new T { Id = entity.Id, Name = entity.Name ?? string.Empty, /* ... */ };
}
```

## 5. Acceptance Criteria

### AC-001: No Command/Query inlines domain fields
Given all Command/Query records in `Module/Features/`, when checked for inlined properties, then zero records have properties other than `Request`, `Id`, `Slug`, or `Parameters`.

### AC-002: No standalone Response records
Given all `record Response` definitions in `Module/Features/`, when checked for inheritance, then zero Responses lack a base type.

### AC-003: No standalone Request records
Given all `record Request` definitions in `Module/Features/`, when checked for inheritance, then zero Requests lack a base type (except features with no body).

### AC-004: No manual Response construction in handlers
Given all Handler classes in `Module/Features/`, when checked for `new Response { }` or `new Response(`, then zero Handlers construct a Response directly (excluding features with no domain entity mapping).

### AC-005: File-upload commands wrap IFormFile in Request
Given all Command records with `IFormFile` parameters, when checked for wrapping, then zero Commands carry `IFormFile` directly.

### AC-006: Create endpoints use `Result<T>.Created()`
Given all create-feature Handlers, when checked for return value construction, then all use `Result<T>.Created(...)` not implicit conversion.

## 6. Test Automation Strategy

- **Test Levels**: Static analysis (Roslyn analyzer or grep-based CI check) for convention compliance.
- **Automation**: Each criterion (AC-001 through AC-006) maps to a bash-based CI check using `grep`/`rg` patterns. New violations block PR merge.
- **CI/CD Integration**: Convention compliance checks run in `.github/workflows/ci.yml` after build, before test.
- **Coverage**: Not applicable (structural convention, not logic).

### Convention check commands (add to CI)

```bash
# AC-001: Find Commands/Queries with inlined fields (not Request/Id/Slug/Parameters)
rg -n 'sealed record (Command|Query)\(' --include '*.cs' service/Api/src/Module/ \
  | rg -v '\((Request|Guid Id|string Slug|Parameters Parameters)' \
  || echo "AC-001 PASS"

# AC-002: Find Response records without base class
rg -n 'public sealed? record Response' --include '*.cs' service/Api/src/Module/ \
  | rg -v 'Response :' \
  || echo "AC-002 PASS"

# AC-003: Find Request records without base class (within feature dirs)
rg -n 'public record Request' --include '*.cs' service/Api/src/Module/Features/ \
  | rg -v 'Request :' \
  || echo "AC-003 PASS"

# AC-004: Find new Response { in handlers
rg -n 'new Response' --include '*.cs' service/Api/src/Module/ \
  | rg 'MapToDetail|MapToListItem|MapToList' \
  || echo "AC-004 check requires manual review"

# AC-005: Find Command with IFormFile directly
rg -n 'IFormFile' --include '*.cs' service/Api/src/Module/ \
  | rg 'sealed record Command\(' \
  || echo "AC-005 PASS"
```

## 7. Rationale & Context

- **Why wrap Request instead of inlining?** A `Command(Request Request)` is self-documenting at the call site: `new Command(request)` vs `new Command(name, slug, price, ...)`. It also enables generic Handler middleware (logging, validation, auditing) that can unwrap `command.Request`.
- **Why inherit Response from a base?** The `MapToDetail<T>()` generic method requires `where T : BaseResponse, new()`. Without inheritance, the mapping method cannot construct the Response type.
- **Why module-internal bases instead of Shared assembly?** Moving every `{Entity}Parameters` into `Shared/` would create a monolithic model assembly and violate module autonomy. The `Shared/` assembly owns only cross-cutting infrastructure (ICommand, Result, Error). Domain model shapes are owned by each module.
- **Why no manual `new Response` in handlers?** Manual construction scatters property-mapping logic across 24+ handlers instead of centralizing it in `{Entity}.Mapping.Model.cs`. A single mapping file is auditable; 24 scattered mappings are not.
- **Status code convention**: `Result<T>.Created(...)` for creates is semantically correct (201 Created vs 200 OK) and consistent with HTTP semantics.

## 8. Dependencies & External Integrations

### Technology Platform Dependencies
- **PLT-001**: .NET 10 (C# 13) — `sealed record`, primary constructors, `required` modifier.
- **PLT-002**: MediatR 12+ — `IRequest<T>`, `IRequestHandler<T, TResponse>`.
- **PLT-003**: Carter 8+ — `ICarterModule` for minimal API endpoints.
- **PLT-004**: FluentValidation 11+ — feature-level validators.

### Infrastructure Dependencies
- **INF-001**: The existing `Shared/Application/Mediators/` interfaces (`ICommand<T>`, `ICommand`, `IQuery<T>`, `IPagedQuery<T>`) must remain stable.

## 9. Examples & Edge Cases

### Correct: Create feature (Catalog)

```csharp
// CreateProduct.cs
public static partial class CreateProduct
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(...) : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken ct)
        {
            var request = command.Request;
            var result = request.MapToDomain();
            if (result.IsFailure) return result.Errors;
            var product = result.Value;
            dbContext.Set<Product>().Add(product);
            await dbContext.SaveChangesAsync(ct);
            return Result<Response>.Created(
                product.MapToDetail<Response>(),
                ProductResult.Success.Created(product.Id));
        }
    }
}

// CreateProduct.Request.cs
public static partial class CreateProduct
{
    public record Request : ProductRequest;
}

// CreateProduct.Response.cs
public static partial class CreateProduct
{
    public record Response : ProductDetailResponse;
}
```

### Correct: ID-only command (Ordering)

```csharp
// ApproveOrder.cs
public static partial class ApproveOrder
{
    public sealed record Command(Guid Id) : ICommand<Response>;
    // Handler uses id to fetch, validate, update, return entity.MapToDetail<Response>()
}
```

### Correct: Paged query

```csharp
// GetPagedOrders.cs
public static partial class GetPagedOrders
{
    public record Query(Parameters Parameters) : IPagedQuery<Response>;
    // Handler uses Parameters for filtering/paging
}
```

### Edge case: File upload (must wrap IFormFile)

```csharp
// SearchByImage.Request.cs
public static partial class SearchByImage
{
    public record Request
    {
        public required IFormFile Image { get; init; }
    }
    // SearchByImage.cs
    public sealed record Command(Request Request) : IQuery<Response>;
}
```

### Edge case: Feature with no domain entity (image serving)

Exception to RQ-004: image serving endpoints that return raw bytes may construct Response directly if no mapping method is meaningful. Such cases must be explicitly marked with a comment:
```csharp
// EXCEPTION: no domain entity — direct construction intentional
return new Response { ... };
```

## 10. Validation Criteria

1. All automated convention checks (AC-001 through AC-005) pass in CI.
2. Each remediated feature is verified by re-reading its files after changes.
3. `dotnet build` succeeds with zero warnings (`TreatWarningsAsErrors=true`).
4. Existing unit tests pass for each modified module.

## 11. Inventory of Deviations

### Category A: Command/Query inlines fields (7 files)

| File | Record | Fix |
|---|---|---|
| `Catalog/Storefront/Products/SearchByImage/SearchByImage.cs:11` | `Command(IFormFile Image)` | Extract `Request` carrying `IFormFile Image` |
| `Inventory/Admin/StockItems/Import/ImportStockItems.cs:9` | `Command(IFormFile File)` | Extract `Request` carrying `IFormFile File` |
| `Inventory/Storefront/StockAvailability/Check/GetStockAvailability.cs:11` | `Query(Guid VariantId, string? CartToken)` | Move `CartToken` into a `Request` record |
| `Inventory/Admin/StockItems/LowStock/GetLowStockItems.cs:8` | `Query(Guid? LocationId, int? Threshold)` | Move `Threshold` into a `Request` record |
| `Inventory/Storefront/CartReservations/Reserve/ReserveCartStock.cs:11` | `Command(Request Request, string CartToken)` | Move `CartToken` into the existing `Request` |
| `Catalog/Admin/Products/Variants/Images/Embeddings/Create/ImageEmbedding.Create.cs:9` | `Command(Guid VariantImageId, string ModelName)` | Move `ModelName` into a `Request` |
| `Catalog/Admin/Products/Variants/Images/Embeddings/Regenerate/ImageEmbedding.Regenerate.cs:9` | `Command(Guid VariantImageId, string ModelName, string ModelVersion)` | Move `ModelName`, `ModelVersion` into a `Request` |

### Category B: Response lacks base type (23 files)

| Module | Feature Files |
|---|---|
| Shipping (3) | `ListShippingRates.Response.cs`, `GetShippingMethods.Response.cs`, `CalculateShipping.Response.cs` |
| Profile (3) | `UpdateNotificationPreferences.Response.cs`, `GetNotificationPreferences.Response.cs`, `DeleteAddress.Response.cs` |
| Identity (3) | `RequestPasswordReset.cs:14`, `GetSession.Response.cs`, `EmailRegister.Response.cs`, `GetUserRoles.Response.cs` |
| Inventory (2) | `GetStockAvailability.Response.cs`, `ImportStockItems.Response.cs` |
| Catalog (11) | `RepositionTaxonUseCase.Response.cs`, `SyncTaxonRules.Response.cs`, `ListVariantsByProduct.Response.cs`, `GetProductClassifications.Response.cs`, `GetVariantOptionValues.Response.cs`, `ListVariantImages.Response.cs`, `DeleteVariantImage.Response.cs`, `SetVariantPrice.Response.cs`, `SearchByImage.Response.cs`, `GetSimilarProducts.Response.cs`, `GetImage.cs:11` |

### Category C: Response inherits from non-Shared service model (2 files)

| File | Current Base | Fix |
|---|---|---|
| `Inventory/Admin/StockItems/Restock/RestockStockItem.Response.cs:7` | `: RestockResult` | Create `StockItemDetailResponse` in Shared models |
| `Inventory/Admin/StockItems/Summary/GetStockSummary.Response.cs:7` | `: VariantStockSummary` | Create `StockSummaryResponse` in Shared models |

### Category D: Request lacks base type (7 files, all Identity)

| File | Fix |
|---|---|
| `Identity/Store/Emails/Resend/ResendEmailVerification.Request.cs:10` | Inherit from `EmailRequest` or define `EmailParameters` base |
| `Identity/Store/Emails/Confirm/ConfirmEmail.Request.cs:12` | Same |
| `Identity/Store/Emails/Change/ChangeEmail.Request.cs:11` | Same |
| `Identity/Store/Auth/Register/EmailRegister.Request.cs:5` | Inherit from `RegisterRequest` base |
| `Identity/Store/Passwords/Change/ChangePassword.Request.cs:11` | Inherit from `PasswordRequest` base |
| `Identity/Store/Passwords/Forgot/RequestPasswordReset.Request.cs:10` | Same |
| `Identity/Store/Passwords/Reset/ResetPassword.Request.cs:12` | Same |

### Category E: Handler manually constructs Response (24 handlers)

Each handler listed in the investigator output must be refactored to use a `MapToDetail<T>()` / `MapToList<T>()` method. Where no mapping method exists, one must be added to the feature's `Shared/Mappings/{Entity}.Mapping.Model.cs` file.

## 12. Related Specifications / Further Reading

- [design-system-domain.md](./design-system-domain.md) — Domain design system
- `docs/codebase/CONVENTIONS.md` — Coding conventions
- `docs/codebase/ARCHITECTURE.md` — Architecture and layer responsibilities
- `service/Api/src/Shared/Application/Mediators/` — Mediator interface definitions
