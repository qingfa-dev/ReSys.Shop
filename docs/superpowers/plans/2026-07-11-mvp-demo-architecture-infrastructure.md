# MVP Demo Architecture Isolation & Infrastructure Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restore module isolation, remove duplicate code, harden security configuration, and make the S3 storage provider and production startup safe for an MVP demo.

**Architecture:** Break direct entity navigation properties between modules, move cross-module service abstractions to `Shared`, consolidate duplicate payment service implementations, and move secrets/migrations out of committed config.

**Tech Stack:** .NET 10, EF Core, Npgsql, AWSSDK.S3, Carter minimal APIs, MediatR, ASP.NET Core Identity, xUnit, FluentAssertions

## Global Constraints

- All domain operations return `Result<T>` or `Result`; exceptions only for unrecoverable infrastructure failures.
- Modules never reference each other; communication via MediatR `ISender` only.
- Every C# feature action is a `static partial class` split across files.
- `TreatWarningsAsErrors=true` globally.
- Forward-only dependency: `Shared` depends on nothing within `service/`. `Module` depends only on `Shared`. `Api` composes both.

---

### Task 1: Remove Direct Identity → Profile Reference in `ConfirmEmail`

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Store/Emails/Confirm/ConfirmEmail.cs`
- Create: `service/Api/src/Shared/Application/Contracts/Profile/CreateUserProfileCommand.cs`
- Modify: `service/Api/src/Module/Profile/Features/Store/Profiles/Create/CreateProfile.cs` (add handler for shared command)
- Test: `service/Api/tests/Module.UnitTests/Architecture/ModuleIsolationTests.cs`

**Interfaces:**
- Consumes: `IMediator`, shared `CreateUserProfileCommand`
- Produces: profile created without Identity referencing Profile types

- [ ] **Step 1: Write the failing architecture test assertion**

The existing test already fails with:

```text
Module.Identity.Features.Store.Emails.Confirm.ConfirmEmail+CommandHandler references Module.Profile...
```

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~ModuleIsolationTests"`

Expected: FAIL

- [ ] **Step 2: Create shared cross-module command**

Create `service/Api/src/Shared/Application/Contracts/Profile/CreateUserProfileCommand.cs`:

```csharp
using Shared.Application.Messaging;

namespace Shared.Application.Contracts.Profile;

public sealed record CreateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string? LastName,
    string Email) : ICommand<CreateUserProfileResult>;

public sealed record CreateUserProfileResult(Guid ProfileId);
```

- [ ] **Step 3: Add handler in Profile module**

In `service/Api/src/Module/Profile/Features/Store/Profiles/Create/CreateProfile.cs`, add a second handler:

```csharp
public sealed class CreateUserProfileCommandHandler(
    IApplicationDbContext dbContext,
    ILogger<CreateUserProfileCommandHandler> logger)
    : ICommandHandler<Shared.Application.Contracts.Profile.CreateUserProfileCommand,
                      Shared.Application.Contracts.Profile.CreateUserProfileResult>
{
    public async Task<Result<Shared.Application.Contracts.Profile.CreateUserProfileResult>> Handle(
        Shared.Application.Contracts.Profile.CreateUserProfileCommand command,
        CancellationToken cancellationToken)
    {
        var inner = new CommandHandler(dbContext, logger);
        var result = await inner.Handle(
            new Command(command.UserId, new Request
            {
                FirstName = command.FirstName,
                LastName = command.LastName ?? string.Empty,
                Email = command.Email
            }), cancellationToken);

        return result.IsSuccess
            ? new Shared.Application.Contracts.Profile.CreateUserProfileResult(result.Value.Id)
            : result.Errors;
    }
}
```

- [ ] **Step 4: Replace direct Profile call in Identity**

In `service/Api/src/Module/Identity/Features/Store/Emails/Confirm/ConfirmEmail.cs`:

```csharp
private async Task CreateUserProfileAsync(User user, CancellationToken cancellationToken)
{
    try
    {
        var profileResult = await mediator.Send(
            new Shared.Application.Contracts.Profile.CreateUserProfileCommand(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email!),
            cancellationToken);

        if (profileResult.IsFailure)
        {
            var errors = string.Join("; ", profileResult.Errors.Select(e => $"{e.Code}: {e.Message}"));
            UserProfileLoggers.Management.ProfileCreationFailed(logger, user.Id, errors);
        }
        else
        {
            UserProfileLoggers.Management.ProfileCreated(logger, user.Id, profileResult.Value.ProfileId);
        }
    }
    catch (Exception ex)
    {
        UserProfileLoggers.Management.ProfileCreationFailed(logger, user.Id, ex.Message);
    }
}
```

Remove the `using Module.Profile...` statements.

- [ ] **Step 5: Run architecture test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~ModuleIsolationTests"`

Expected: Identity → Profile violation removed

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Shared/Application/Contracts/Profile/CreateUserProfileCommand.cs
git add service/Api/src/Module/Profile/Features/Store/Profiles/Create/CreateProfile.cs
git add service/Api/src/Module/Identity/Features/Store/Emails/Confirm/ConfirmEmail.cs
git commit -m "refactor(identity): remove direct Profile reference via shared command"
```

---

### Task 2: Remove Cross-Module Entity Navigation Properties

**Files:**
- Modify: `service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.cs`
- Modify: `service/Api/src/Module/Inventory/Domain/StockLocations/StockItems/StockItem.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/LineItems/LineItem.cs`
- Modify: `service/Api/src/Module/Payment/Persistence/Configurations/PaymentCapture.Configuration.cs` (if Order navigation exists)
- Modify: `service/Api/src/Module/Inventory/Persistence/Configurations/StockItem.Configuration.cs`
- Modify: `service/Api/src/Module/Catalog/Persistence/Configurations/Variant.Configuration.cs`
- Test: `service/Api/tests/Module.UnitTests/Architecture/ModuleIsolationTests.cs`

**Interfaces:**
- Consumes: foreign key identifiers already present on entities
- Produces: entities that reference only IDs, not foreign entity types

- [ ] **Step 1: Remove `StockItem` navigation from `Variant`**

In `service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.cs`, remove:

```csharp
public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
```

Remove the corresponding EF configuration line in `Variant.Configuration.cs`.

- [ ] **Step 2: Remove `Variant` navigation from `StockItem`**

In `service/Api/src/Module/Inventory/Domain/StockLocations/StockItems/StockItem.cs`, remove:

```csharp
public Variant Variant { get; set; } = null!;
```

Ensure only `public Guid VariantId { get; set; }` remains. Remove the relationship configuration in `StockItem.Configuration.cs`.

- [ ] **Step 3: Remove `PaymentCapture` navigation from `Order`**

In `service/Api/src/Module/Ordering/Domain/Orders/Order.cs`, remove:

```csharp
public ICollection<PaymentCapture> Payments { get; set; } = new List<PaymentCapture>();
```

- [ ] **Step 4: Remove `Variant` navigation from `LineItem`**

In `service/Api/src/Module/Ordering/Domain/LineItems/LineItem.cs`, remove:

```csharp
public Variant Variant { get; set; } = null!;
```

Ensure only `public Guid VariantId { get; set; }` remains.

- [ ] **Step 5: Update handlers that used navigations**

Search for `.Include(x => x.Variant)` in Ordering/Inventory handlers and replace with explicit `Variant` queries where needed. For example, in `AddToCart.cs` line 38:

```csharp
var variant = await dbContext.Set<Variant>()
    .FirstOrDefaultAsync(x => x.Id == request.VariantId, cancellationToken);
```

This is already a direct query, so no change is required there; verify other handlers.

- [ ] **Step 6: Run architecture test**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~ModuleIsolationTests"`

Expected: Catalog ↔ Inventory and Ordering → Catalog/Payment violations removed

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Catalog/Domain/Products/Variants/Variant.cs
git add service/Api/src/Module/Catalog/Persistence/Configurations/Variant.Configuration.cs
git add service/Api/src/Module/Inventory/Domain/StockLocations/StockItems/StockItem.cs
git add service/Api/src/Module/Inventory/Persistence/Configurations/StockItem.Configuration.cs
git add service/Api/src/Module/Ordering/Domain/Orders/Order.cs
git add service/Api/src/Module/Ordering/Domain/LineItems/LineItem.cs
git commit -m "refactor: remove cross-module entity navigation properties"
```

---

### Task 3: Move Inventory Service Abstraction to Shared

**Files:**
- Create: `service/Api/src/Shared/Application/Contracts/Inventory/IStockQuantityService.cs`
- Modify: `service/Api/src/Module/Inventory/Services/StockQuantityService.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Shared/Services/OrderInventoryService.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs`
- Test: `service/Api/tests/Module.UnitTests/Architecture/ModuleIsolationTests.cs`

**Interfaces:**
- Consumes: existing `IStockQuantityService` implementation in Inventory
- Produces: `Shared.Application.Contracts.Inventory.IStockQuantityService` consumed by Ordering

- [ ] **Step 1: Define shared interface**

Create `service/Api/src/Shared/Application/Contracts/Inventory/IStockQuantityService.cs`:

```csharp
namespace Shared.Application.Contracts.Inventory;

public interface IStockQuantityService
{
    Task<Result<int>> GetAvailableQuantityAsync(Guid variantId, Guid? stockLocationId = null, CancellationToken ct = default);
    Task<Result<bool>> IsAvailableAsync(Guid variantId, int quantity, CancellationToken ct = default);
}
```

- [ ] **Step 2: Implement shared interface in Inventory**

In `service/Api/src/Module/Inventory/Services/StockQuantityService.cs`:

```csharp
using Shared.Application.Contracts.Inventory;

namespace Module.Inventory.Services;

public sealed class StockQuantityService : IStockQuantityService
{
    // existing implementation
}
```

- [ ] **Step 3: Update Ordering consumers to use shared interface**

In Ordering files, replace `using Module.Inventory.Services.Abstractions;` with `using Shared.Application.Contracts.Inventory;`.

- [ ] **Step 4: Register the service under the shared interface**

In `service/Api/src/Module/Inventory/Inventory.Extensions.cs`, verify registration:

```csharp
services.AddScoped<IStockQuantityService, StockQuantityService>();
```

- [ ] **Step 5: Run architecture test**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~ModuleIsolationTests"`

Expected: Ordering → Inventory service type reference violations removed

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Shared/Application/Contracts/Inventory/IStockQuantityService.cs
git add service/Api/src/Module/Inventory/Services/StockQuantityService.cs
git add service/Api/src/Module/Ordering/Features/
git add service/Api/src/Module/Inventory/Inventory.Extensions.cs
git commit -m "refactor(ordering): depend on shared IStockQuantityService abstraction"
```

---

### Task 4: Consolidate Duplicate Payment Service Implementations

**Files:**
- Delete: `service/Api/src/Module/Payment/Services/Gateways/StripeGateway.cs`
- Delete: `service/Api/src/Module/Payment/Services/Gateways/PaymentProcessingService.cs`
- Delete: `service/Api/src/Module/Payment/Services/Gateways/BogusGateway.cs`
- Delete: `service/Api/src/Module/Payment/Services/Gateways/BogusGateway.Result.cs`
- Delete: `service/Api/src/Module/Payment/Services/Gateways/GatewayRegistry.cs`
- Delete: `service/Api/src/Module/Payment/Services/Webhooks/StripeWebhookService.cs`
- Delete: `service/Api/src/Module/Payment/Services/Models/*` (duplicate models)
- Modify: `service/Api/src/Module/Payment/Payment.Extensions.cs` (verify registrations point to canonical types)
- Test: `dotnet build service/Api/src/Api/Api.csproj`

**Interfaces:**
- Consumes: canonical implementations under `Services/Provider/` and `Services/Webhook/`
- Produces: single implementation per payment service type

- [ ] **Step 1: Verify canonical implementations compile**

Run: `dotnet build service/Api/src/Module/Module.csproj`

Expected: Build succeeds before deleting duplicates

- [ ] **Step 2: Delete duplicate files**

```bash
rm service/Api/src/Module/Payment/Services/Gateways/StripeGateway.cs
rm service/Api/src/Module/Payment/Services/Gateways/PaymentProcessingService.cs
rm service/Api/src/Module/Payment/Services/Gateways/BogusGateway.cs
rm service/Api/src/Module/Payment/Services/Gateways/BogusGateway.Result.cs
rm service/Api/src/Module/Payment/Services/Gateways/GatewayRegistry.cs
rm service/Api/src/Module/Payment/Services/Webhooks/StripeWebhookService.cs
```

Delete duplicate model files if they are exact copies under `Services/Models/`:

```bash
rm service/Api/src/Module/Payment/Services/Models/*.cs
```

- [ ] **Step 3: Update registrations and usings**

Search for `using Module.Payment.Services.Gateways;` and `using Module.Payment.Services.Webhooks;` and replace with canonical namespaces:

```csharp
using Module.Payment.Services.Provider;
using Module.Payment.Services.Provider.Stripe;
using Module.Payment.Services.Provider.Bogus;
using Module.Payment.Services.Webhook;
```

- [ ] **Step 4: Run build**

Run: `dotnet build service/Api/src/Api/Api.csproj`

Expected: 0 warnings, 0 errors

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Services/
git commit -m "refactor(payment): remove duplicate gateway and webhook service implementations"
```

---

### Task 5: Implement S3 Storage Provider

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/...` (skip — not needed here)
- Modify: `service/Api/src/Shared/Operational/Storages/Providers/S3.StorageProvider.Implementation.cs`
- Modify: `service/Api/src/Shared/Shared.csproj`
- Modify: `service/Api/src/Shared/Operational/Storages/Providers/Options/S3StorageProviderSetting.cs`
- Test: `service/Api/tests/Shared.UnitTests/Operational/Storages/Providers/S3StorageProviderTests.cs` (create, possibly with LocalStack/MinIO integration)

**Interfaces:**
- Consumes: `AWSSDK.S3` package, `S3StorageProviderSetting`
- Produces: real `IStorageProvider` implementation for S3-compatible stores

- [ ] **Step 1: Add AWSSDK.S3 package**

In `service/Api/src/Shared/Shared.csproj`:

```xml
<PackageReference Include="AWSSDK.S3" />
```

In `service/Api/Directory.Packages.props`:

```xml
<PackageVersion Include="AWSSDK.S3" Version="3.7.400.0" />
```

- [ ] **Step 2: Implement S3 operations**

Replace the stub in `S3.StorageProvider.Implementation.cs` with real SDK calls. Example shape:

```csharp
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Shared.Operational.Storages.Models;
using Shared.Operational.Storages.Providers.Options;

namespace Shared.Operational.Storages.Providers;

internal sealed partial class S3StorageProvider(
    IAmazonS3 s3Client,
    IOptions<S3StorageProviderSetting> options,
    ILogger<S3StorageProvider> logger)
    : IStorageProvider
{
    public string Name => "s3";

    public async Task<Result<UploadResult>> UploadAsync(UploadRequest request, CancellationToken ct = default)
    {
        var opts = options.Value;
        Loggers.LogUploadStart(logger, request.Key, opts.BucketName);

        var putRequest = new PutObjectRequest
        {
            BucketName = opts.BucketName,
            Key = request.Key.TrimStart('/'),
            InputStream = request.Content,
            AutoCloseStream = false
        };

        var response = await s3Client.PutObjectAsync(putRequest, ct);
        var uri = BuildBucketUri(request.Key);

        return Result<UploadResult>.Ok(new UploadResult(
            request.Key,
            Name,
            uri,
            response.ContentLength,
            DateTimeOffset.UtcNow));
    }

    public async Task<Result<DownloadResult>> DownloadAsync(string key, CancellationToken ct = default)
    {
        var opts = options.Value;
        try
        {
            var response = await s3Client.GetObjectAsync(opts.BucketName, key.TrimStart('/'), ct);
            var ms = new MemoryStream();
            await response.ResponseStream.CopyToAsync(ms, ct);
            ms.Position = 0;
            return Result<DownloadResult>.Ok(new DownloadResult(key, ms, response.ContentLength, response.ContentType));
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound("Storage.ObjectNotFound", $"Object '{key}' not found.");
        }
    }

    public async Task<Result> DeleteAsync(string key, CancellationToken ct = default)
    {
        var opts = options.Value;
        await s3Client.DeleteObjectAsync(opts.BucketName, key.TrimStart('/'), ct);
        return Result.Ok();
    }

    public async Task<Result<StoredObjectInfo>> StatAsync(string key, CancellationToken ct = default)
    {
        var opts = options.Value;
        try
        {
            var response = await s3Client.GetObjectMetadataAsync(opts.BucketName, key.TrimStart('/'), ct);
            return Result<StoredObjectInfo>.Ok(new StoredObjectInfo(
                key,
                response.ContentLength,
                response.LastModified,
                response.ContentType));
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound("Storage.ObjectNotFound", $"Object '{key}' not found.");
        }
    }

    public async Task<Result<IReadOnlyList<StoredObjectInfo>>> ListAsync(string? prefix, CancellationToken ct = default)
    {
        var opts = options.Value;
        var request = new ListObjectsV2Request
        {
            BucketName = opts.BucketName,
            Prefix = prefix
        };

        var response = await s3Client.ListObjectsV2Async(request, ct);
        var objects = response.S3Objects
            .Select(o => new StoredObjectInfo(o.Key, o.Size, o.LastModified, null))
            .ToList();

        return Result<IReadOnlyList<StoredObjectInfo>>.Ok(objects);
    }

    public Result<string> ResolvePath(string key) =>
        StorageResult.Failure.ProviderError("S3 is not a file-based provider. Use DownloadAsync to get a stream.");

    private Uri BuildBucketUri(string key)
    {
        var opts = options.Value;
        if (!string.IsNullOrWhiteSpace(opts.ServiceUrl))
            return new Uri($"{opts.ServiceUrl.TrimEnd('/')}/{opts.BucketName}/{key.TrimStart('/')}");

        return new Uri($"https://{opts.BucketName}.s3.{opts.Region}.amazonaws.com/{key.TrimStart('/')}");
    }
}
```

- [ ] **Step 3: Register AmazonS3 client**

In `service/Api/src/Shared/Operational/Storages/Storage.Extensions.cs`:

```csharp
services.AddSingleton<IAmazonS3>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<S3StorageProviderSetting>>().Value;
    var config = new AmazonS3Config();

    if (!string.IsNullOrWhiteSpace(opts.ServiceUrl))
    {
        config.ServiceURL = opts.ServiceUrl;
        config.ForcePathStyle = opts.ForcePathStyle;
    }
    else if (!string.IsNullOrWhiteSpace(opts.Region))
    {
        config.RegionEndpoint = RegionEndpoint.GetBySystemName(opts.Region);
    }

    return new AmazonS3Client(opts.AccessKey, opts.SecretKey, config);
});
```

- [ ] **Step 4: Run build**

Run: `dotnet build service/Api/src/Api/Api.csproj`

Expected: 0 warnings, 0 errors

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Shared/Shared.csproj
git add service/Api/Directory.Packages.props
git add service/Api/src/Shared/Operational/Storages/
git commit -m "feat(storage): implement S3 storage provider"
```

---

### Task 6: Gate Production Migrations

**Files:**
- Modify: `service/Api/src/Api/Program.cs`
- Modify: `service/Api/src/Api/appsettings.json`
- Modify: `service/Api/src/Api/appsettings.Development.json`
- Test: manual verification by running with `ASPNETCORE_ENVIRONMENT=Production`

**Interfaces:**
- Consumes: `DatabaseInitializationOptions:RunMigrations`
- Produces: migrations only run in production when explicitly enabled

- [ ] **Step 1: Add configuration option**

In `service/Api/src/Api/appsettings.json`:

```json
"DatabaseInitialization": {
  "RunMigrations": false
}
```

In `service/Api/src/Api/appsettings.Development.json`:

```json
"DatabaseInitialization": {
  "RunMigrations": true
}
```

- [ ] **Step 2: Update Program.cs**

```csharp
bool runMigrations = builder.Configuration.GetValue<bool>("DatabaseInitialization:RunMigrations");
bool runSeeders = !app.Environment.IsProduction();
await app.InitializeDatabaseAsync(runMigrations: runMigrations, runSeeders: runSeeders);
```

- [ ] **Step 3: Run build**

Run: `dotnet build service/Api/src/Api/Api.csproj`

Expected: 0 warnings, 0 errors

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Api/Program.cs
git add service/Api/src/Api/appsettings.json
git add service/Api/src/Api/appsettings.Development.json
git commit -m "feat(api): gate production database migrations via configuration"
```

---

### Task 7: Remove Hardcoded Development JWT Secret

**Files:**
- Modify: `service/Api/src/Api/appsettings.Development.json`
- Create: `service/Api/src/Api/README-SECRETS.md` (optional, only if explicitly requested)
- Test: `dotnet build service/Api/src/Api/Api.csproj` and run with missing secret to verify graceful failure

**Interfaces:**
- Consumes: user secrets / environment variables
- Produces: no committed JWT secret

- [ ] **Step 1: Remove secret from committed config**

In `service/Api/src/Api/appsettings.Development.json`, change:

```json
"Jwt": {
  "Secret": ""
}
```

- [ ] **Step 2: Ensure validation fails loudly if secret is missing**

Verify `Shared.Security.Authentication.Tokens.Options.JwtSetting.Validator.cs` rejects empty secrets. If not, add:

```csharp
RuleFor(x => x.Secret)
    .NotEmpty()
    .MinimumLength(32)
    .WithMessage("JWT secret must be configured via user secrets or environment variables.");
```

- [ ] **Step 3: Document local setup**

Run from `service/Api/src/Api`:

```bash
dotnet user-secrets init
dotnet user-secrets set "Authentication:Jwt:Secret" "YourLocalDevSecretThatIsAtLeast32Chars!"
```

- [ ] **Step 4: Commit config change only**

```bash
git add service/Api/src/Api/appsettings.Development.json
git commit -m "security(api): remove hardcoded JWT secret from development config"
```

---

### Task 8: Remove Obsolete Domain Guards

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Checkout.cs`
- Test: `dotnet build service/Api/src/Module/Module.csproj`

**Interfaces:**
- Consumes: existing call sites
- Produces: clean domain model without misleading always-true guards

- [ ] **Step 1: Delete or implement obsolete guards**

In `service/Api/src/Module/Ordering/Domain/Orders/Order.Checkout.cs`:

Remove:

```csharp
[Obsolete("Stock validation is handled in CreateOrderFromCart handler")]
internal bool EnsureLineItemsAreInStock()
{
    return true;
}
```

Remove:

```csharp
[Obsolete("Shipping rate validation handled in UpdateCheckout handler")]
internal bool EnsureAvailableShippingRates()
{
    return true;
}
```

- [ ] **Step 2: Update callers**

Search for usages of these methods and remove the calls.

- [ ] **Step 3: Run build**

Run: `dotnet build service/Api/src/Module/Module.csproj`

Expected: 0 warnings, 0 errors

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/Orders/Order.Checkout.cs
git commit -m "refactor(ordering): remove obsolete always-true domain guards"
```

---

### Task 9: Final Verification

- [ ] **Step 1: Run architecture test**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~ModuleIsolationTests"`

Expected: PASS

- [ ] **Step 2: Run full Module unit test suite**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj`

Expected: All tests pass

- [ ] **Step 3: Run Shared unit tests**

Run: `dotnet test service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj`

Expected: All tests pass

- [ ] **Step 4: Run build with warnings-as-errors**

Run: `dotnet build service/Api/src/Api/Api.csproj`

Expected: 0 warnings, 0 errors

- [ ] **Step 5: Commit**

```bash
git commit -m "chore: final verification for architecture and infrastructure hardening" --allow-empty
```
