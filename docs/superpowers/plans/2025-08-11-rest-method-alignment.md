# REST Method Alignment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix HTTP method violations across Orders, Shipping, and Customer account endpoints. Move `GET /customer/all` to admin with permission check — security fix for data leak.

**Architecture:** Correct HTTP methods: cancel order → POST, shipping calculate → GET, 5 customer PUT endpoints → PATCH. Move `/customer/all` to `ProfileFeature.Admin.cs` with `DashboardFeatureMetadata.Customer.List` permission. Order and shipping route prefixes already fixed in cart consolidation plan.

**Tech Stack:** .NET 10, C#, Carter minimal APIs, FluentValidation

## Global Constraints

- .NET 10, TreatWarningsAsErrors=true
- `PATCH` for partial updates, `POST` for actions, `GET` for idempotent reads
- Admin routes use `.HasPermission()` with `DashboardFeatureMetadata.*` or `{Module}FeatureMetadata.*`
- `dotnet build` must pass after each task

---

### Task 1: Fix Orders — Cancel Method + Variant IDs in route

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.Endpoint.cs` (change `.MapPut` → `.MapPost`)
- (Route prefix `ordering/orders` → `orders` already handled by Task 4 of cart consolidation plan — just verify)

- [ ] **Step 1: Change CancelOrder endpoint method**

```bash
# Verify the endpoint file method
rg "MapPut|MapPost" service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.Endpoint.cs
```

Edit file: change `.MapPut(...)` to `.MapPost(...)`. The route constant and handler logic stay unchanged — only the HTTP method.

- [ ] **Step 2: Build**

```bash
dotnet build
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.Endpoint.cs
git commit -m "fix(ordering): change cancel order from PUT to POST

Cancel is a non-idempotent action — POST is the correct HTTP method."
```

### Task 2: Fix Shipping — Calculate Method from POST to GET

**Files:**
- Modify: `service/Api/src/Module/Shipping/Features/Shared/ShippingFeature.Storefront.cs`
- Modify: `service/Api/src/Module/Shipping/Features/Storefront/Shipping/Calculate/CalculateShipping.Endpoint.cs`

- [ ] **Step 1: Remove Calculate from Storefront.cs POST-style, add as GET**

In `ShippingFeature.Storefront.cs`, keep the class but update Summary/Description to indicate it's a GET read operation:

```csharp
public static class Calculate
{
    public const string Route = "api/storefront/shipping/calculate";
    public const string Description = "Calculate shipping cost for an order and method";
    public const string Summary = "Calculate shipping cost";
}
```

- [ ] **Step 2: Update CalculateShipping.Endpoint.cs**

Change `.MapPost(...)` to `.MapGet(...)`.
Change request body parameters to query parameters:
- `shippingMethodId` → `[FromQuery] Guid shippingMethodId`
- `orderId` → `[FromQuery] Guid orderId`

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Shipping/Features/Shared/ShippingFeature.Storefront.cs
git add service/Api/src/Module/Shipping/Features/Storefront/Shipping/Calculate/CalculateShipping.Endpoint.cs
git commit -m "fix(shipping): change calculate from POST to GET

Shipping calculation is an idempotent computation — GET is correct.
Query params: shippingMethodId, orderId."
```

### Task 3: Fix Customer Account — PUT → PATCH for 5 Endpoints

**Files:**
- Modify: `service/Api/src/Module/Customer/Features/Storefront/` — find all PUT-based endpoint files

- [ ] **Step 1: Find all customer endpoints using PUT**

```bash
rg "MapPut" service/Api/src/Module/Customer/Features/Storefront/ --no-heading
```

Expected results: profile update, address create (POST, OK), address update, address set-default, wishlist update, notification preferences update.

- [ ] **Step 2: Change PUT → PATCH on each**

For each `.MapPut(...)` in customer storefront endpoints:
- Change `.MapPut` to `.MapPatch`
- No handler or route constant changes needed

Files to modify:
- `Customer/Update/UpdateCustomer.Endpoint.cs` (or similar name)
- `Addresses/Update/*.Endpoint.cs`
- `Addresses/SetDefault/*.Endpoint.cs` — also change PUT → PATCH
- `Wishlists/Update/*.Endpoint.cs`
- `NotificationPreferences/Update/*.Endpoint.cs` — also change PUT → PATCH

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Customer/Features/Storefront/
git commit -m "fix(customer): change 5 PUT endpoints to PATCH

Profile, address, wishlist, notification preferences — all accept
partial updates (not full replacement). PATCH is correct."
```

### Task 4: Remove Storefront GET /customer/all — Admin Endpoint Already Exists

**Files:**
- Modify: `service/Api/src/Module/Customer/Features/Shared/ProfileFeature.Storefront.cs` (remove orphaned `/all` route constant)
- Modify: `service/Api/src/Module/Customer/Features/Admin/Profiles/Get/PagedOrAll/GetUserProfilesPagedOrAll.Endpoint.cs` (fix metadata refs)

**FINDING (pre-flight review):** The admin endpoint `GET /api/admin/customer/all` ALREADY EXISTS in `GetUserProfilesPagedOrAll.Endpoint.cs` with `.RequireAuthorization()` + `.HasPermission(ProfileFeature.Admin.Profiles.GetAll.Permission)`. The storefront `Profiles.GetAll` route constant (`api/storefront/customer/all`) is ORPHANED — no endpoint file references it, so the data-leak endpoint does not actually exist at runtime. The security fix is to delete the dead constant and fix the admin endpoint's metadata which incorrectly points at the storefront constants.

- [ ] **Step 1: Remove orphaned storefront GetAll constant**

In `ProfileFeature.Storefront.cs`, delete the `public static class GetAll` block (the one with `Route = "api/storefront/customer/all"`). The route was never wired to an endpoint, so removing it closes the theoretical exposure with zero runtime behavior change.

- [ ] **Step 2: Fix admin endpoint metadata references**

In `GetUserProfilesPagedOrAll.Endpoint.cs`, lines 35-36 currently read:

```csharp
.WithSummary(ProfileFeature.Storefront.Profiles.GetAll.Summary)
.WithDescription(ProfileFeature.Storefront.Profiles.GetAll.Description)
```

After deleting the storefront constant, these references break. Change them to use the Admin constants (which already exist in `ProfileFeature.Admin.cs`):

```csharp
.WithSummary(ProfileFeature.Admin.Profiles.GetAll.Summary)
.WithDescription(ProfileFeature.Admin.Profiles.GetAll.Description)
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Customer/Features/Shared/ProfileFeature.Storefront.cs
git add service/Api/src/Module/Customer/Features/Admin/Profiles/Get/PagedOrAll/GetUserProfilesPagedOrAll.Endpoint.cs
git commit -m "fix(customer): remove orphaned storefront /customer/all route

Admin endpoint GET /api/admin/customer/all already exists with
RequireAuthorization + HasPermission. The storefront constant was
dead code exposing a route never wired to a handler. Deleted it and
fixed the admin endpoint's summary/description to reference Admin
constants. Security posture unchanged at runtime — no data leak existed."
```

### Task 5: Full Build + Tests

- [ ] **Step 1: Full build**

```bash
dotnet build
```

- [ ] **Step 2: Unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests
```

- [ ] **Step 3: Feature conventions check**

```bash
bash scripts/check-feature-conventions.sh
```

- [ ] **Step 4: Commit**

```bash
git commit --allow-empty -m "chore: verify full build and tests after REST method alignment"
```
