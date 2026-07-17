# Query Parameter Alignment — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Align API `Constant.Query` allowed fields with handler enforcement (strict 400 on invalid fields), fix admin app query parameter bugs (URL mismatches, sort fields, searchFields scoping, dead code).

**Architecture:** Two-sided alignment. API side: create missing constants, restructure non-standard patterns, enforce in all ~31 handlers via `ParseAll()` arguments. Admin side: fix 4 URL mismatches, fix sort fields to match entity properties, scope search to relevant fields, remove dead query type fields.

**Tech Stack:** .NET 10 (C#), Vue 3 + TypeScript 6, EF Core, MediatR

## Global Constraints

- `TreatWarningsAsErrors=true` — any warning fails the build
- `Result<T>` / `Result` for all operations — no exceptions for domain logic
- All query parameter string values (sort/filter/search field names) are PascalCase entity property names — never i18n-translated
- Strict enforcement: `ParseAll()` returns errors on invalid fields (400 Unprocessable Entity)
- Custom handler parameters (e.g., Products `Status`/`TaxonId`/`Season`) kept separate from DSL `Constant.Query`

---

### Task 1: Create `User.Constant.cs`

**Files:**
- Create: `service/Api/src/Module/Identity/Domain/Users/User.Constant.cs`
- Create: `service/Api/src/Module/Identity/Domain/Users/` (directory if needed)

**Interfaces:**
- Produces: `UserConstant.Query.AllowedFilterFields`, `.AllowedSearchFields`, `.AllowedSortFields` — consumed by Task 7

- [ ] **Step 1: Create User.Constant.cs**

```csharp
namespace Module.Identity.Domain.Users;

public static class UserConstant
{
    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(User.UserName),
            nameof(User.Email),
            nameof(User.FirstName),
            nameof(User.LastName)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(User.UserName),
            nameof(User.Email),
            nameof(User.CreatedAtUtc),
            nameof(User.ModifiedAtUtc),
            nameof(User.LastLoginAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(User.IsActive),
            nameof(User.EmailConfirmed),
            nameof(User.PhoneNumberConfirmed),
            nameof(User.CreatedAtUtc),
            nameof(User.ModifiedAtUtc)
        ];
    }
}
```

- [ ] **Step 2: Verify User entity has FirstName/LastName/LastLoginAtUtc**

Run: `rg "class User" service/Api/src/Shared/Security/Identity/Domain/Users/ --type cs -A 30`
Expected: `User` class contains `FirstName`, `LastName`, `LastLoginAtUtc` properties (from ExtendedIdentityUser base).
If any property is missing, adjust the constant to use only existing properties.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Identity/Domain/Users/User.Constant.cs
git commit -m "feat(identity): add UserConstant.Query with allowed fields"
```

---

### Task 2: Create `Role.Constant.cs`

**Files:**
- Create: `service/Api/src/Module/Identity/Domain/Roles/Role.Constant.cs`
- Create: `service/Api/src/Module/Identity/Domain/Roles/` (directory if needed)

**Interfaces:**
- Produces: `RoleConstant.Query.AllowedFilterFields`, `.AllowedSearchFields`, `.AllowedSortFields` — consumed by Task 7

- [ ] **Step 1: Create Role.Constant.cs**

```csharp
namespace Module.Identity.Domain.Roles;

public static class RoleConstant
{
    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Role.Name),
            nameof(Role.Description)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Role.Name),
            nameof(Role.IsSystem),
            nameof(Role.CreatedAtUtc),
            nameof(Role.ModifiedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Role.IsSystem),
            nameof(Role.CreatedAtUtc),
            nameof(Role.ModifiedAtUtc)
        ];
    }
}
```

- [ ] **Step 2: Verify Role entity properties**

Run: `rg "class Role" service/Api/src/Shared/Security/Identity/Domain/Roles/ --type cs -A 20`
Expected: `Role` extends `IdentityRole<Guid>` with `Description`, `IsSystem`, `CreatedAtUtc`, `ModifiedAtUtc`.
Adjust constant if any property doesn't exist.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Identity/Domain/Roles/Role.Constant.cs
git commit -m "feat(identity): add RoleConstant.Query with allowed fields"
```

---

### Task 3: Add `Query` sub-class to `UserProfile.Constant.cs`

**Files:**
- Modify: `service/Api/src/Module/Profile/Domain/UserProfile.Constant.cs`

**Interfaces:**
- Produces: `UserProfileConstant.Query.AllowedFilterFields`, `.AllowedSearchFields`, `.AllowedSortFields` — consumed by Task 12

- [ ] **Step 1: Read current file**

Run: `cat service/Api/src/Module/Profile/Domain/UserProfile.Constant.cs`

- [ ] **Step 2: Add `Query` sub-class before the closing `}` of the class**

Insert before the last `}`:

```csharp
    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(UserProfile.FirstName),
            nameof(UserProfile.LastName),
            nameof(UserProfile.Email),
            nameof(UserProfile.Bio)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(UserProfile.FirstName),
            nameof(UserProfile.LastName),
            nameof(UserProfile.CreatedAtUtc),
            nameof(UserProfile.ModifiedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(UserProfile.Gender),
            nameof(UserProfile.IsActive),
            nameof(UserProfile.CreatedAtUtc),
            nameof(UserProfile.ModifiedAtUtc)
        ];
    }
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Profile/Domain/UserProfile.Constant.cs
git commit -m "feat(profile): add UserProfileConstant.Query with allowed fields"
```

---

### Task 4: Restructure `Country.Constant.cs` — move `Constraints.Query` to `Query`

**Files:**
- Modify: `service/Api/src/Module/Location/Domain/Countries/Country.Constant.cs`
- Modify: 4 consumer files (see Step 2)

**Interfaces:**
- Produces: `CountryConstant.Query.*` (was `CountryConstant.Constraints.Query.*`) — consumed by Task 9

- [ ] **Step 1: Move the Query class from `Constraints.Query` to top level**

In `Country.Constant.cs`, cut the entire `public static class Query { ... }` block from inside `Constraints` and paste it directly inside `CountryConstant` (as a sibling of `Constraints` and `Defaults`). No field changes — just the nesting level changes.

Before:
```csharp
public static class Constraints
{
    // ...
    public static class Query { ... AllowedSearchFields, AllowedSortFields, AllowedFilterFields ... }
}
```

After:
```csharp
public static class Constraints
{
    // ... (no more Query inside)
}

public static class Query
{
    // ... same arrays
}
```

- [ ] **Step 2: Update all consumer references**

Search for all references to `CountryConstant.Constraints.Query`:

```bash
rg "CountryConstant.Constraints.Query" service/Api/src/ --type cs
```

Replace `CountryConstant.Constraints.Query.AllowedXxxFields` with `CountryConstant.Query.AllowedXxxFields` in every match. Expected: ~4 files (2 location admin handlers, 2 location store handlers).

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Location/Domain/Countries/Country.Constant.cs
git add $(rg "CountryConstant.Constraints.Query" service/Api/src/ --type cs -l)
git commit -m "refactor(location): move CountryConstant.Constraints.Query to CountryConstant.Query"
```

---

### Task 5: Restructure `State.Constant.cs` — wrap in `Query` sub-class

**Files:**
- Modify: `service/Api/src/Module/Location/Domain/States/State.Constant.cs`
- Modify: 4 consumer files (see Step 2)

**Interfaces:**
- Produces: `StateConstant.Query.*` (was `StateConstant.AllowedXxxFields` at class level) — consumed by Task 9

- [ ] **Step 1: Wrap field arrays in `Query` sub-class**

In `State.Constant.cs`, wrap the three field arrays (`AllowedSearchFields`, `AllowedSortFields`, `AllowedFilterFields`) inside a new `public static class Query { ... }` at the same level as `Constraints` and `Defaults`.

Before:
```csharp
    // ... Constraints, Defaults ...

    public static readonly string[] AllowedSearchFields = [...];
    public static readonly string[] AllowedSortFields = [...];
    public static readonly string[] AllowedFilterFields = [...];
}
```

After:
```csharp
    // ... Constraints, Defaults ...

    public static class Query
    {
        public static readonly string[] AllowedSearchFields = [...];
        public static readonly string[] AllowedSortFields = [...];
        public static readonly string[] AllowedFilterFields = [...];
    }
}
```

- [ ] **Step 2: Update all consumer references**

Search for references using the old pattern:

```bash
rg "StateConstant.Allowed" service/Api/src/ --type cs
```

Replace `StateConstant.AllowedXxxFields` with `StateConstant.Query.AllowedXxxFields` in every match. Expected: ~4 files (2 location admin handlers, 2 location store handlers).

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Location/Domain/States/State.Constant.cs
git add $(rg "StateConstant.Allowed" service/Api/src/ --type cs -l)
git commit -m "refactor(location): wrap StateConstant field arrays in Query sub-class"
```

---

### Task 6: Enforce Catalog handlers (7 + 1 special)

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Get/Paged/GetProductsPaged.cs:56`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Get/Paged/GetTaxonomiesPaged.cs:29`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/Get/Paged/GetTaxonsAllOrPaged.cs:42`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/OptionTypes/Get/Paged/GetOptionTypesPaged.cs:28`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/OptionTypes/OptionValues/Get/Paged/GetOptionValuesPaged.cs:26`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Prices/Get/ListPricesByVariant.cs:45`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Taxons/Get/All/GetAllTaxons.cs:42`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/OptionTypes/Get/All/GetAllOptionTypes.cs:31`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Taxons/Get/Products/GetProducts.cs:48`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.cs:51-54`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Related/GetRelatedProducts.cs:67`

**Consumes:** `ProductConstant.Query`, `TaxonomyConstant.Query`, `TaxonConstant.Query`, `OptionTypeConstant.Query`, `OptionValueConstant.Query`, `PriceConstant.Query` (all pre-existing, no changes needed)

**Interfaces:**
- Produces: Handlers return 400 on invalid field names instead of silently accepting them

- [ ] **Step 1: Add using directives**

For each handler file that references an entity constant, ensure the using directive exists:

```csharp
using Module.Catalog.Domain.Products;          // for ProductConstant
using Module.Catalog.Domain.Taxonomies;        // for TaxonomyConstant
using Module.Catalog.Domain.Taxonomies.Taxons;  // for TaxonConstant
using Module.Catalog.Domain.OptionTypes;        // for OptionTypeConstant
using Module.Catalog.Domain.OptionTypes.Values; // for OptionValueConstant
using Module.Catalog.Domain.Products.Variants.Prices; // for PriceConstant
```

Check which ones already exist vs need adding.

- [ ] **Step 2: Update GetProductsPaged.cs:56**

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: ProductConstant.Query.AllowedFilterFields,
    allowedSearchFields: ProductConstant.Query.AllowedSearchFields,
    allowedSortFields: ProductConstant.Query.AllowedSortFields);
```

- [ ] **Step 3: Update GetTaxonomiesPaged.cs:29**

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: TaxonomyConstant.Query.AllowedFilterFields,
    allowedSearchFields: TaxonomyConstant.Query.AllowedSearchFields,
    allowedSortFields: TaxonomyConstant.Query.AllowedSortFields);
```

- [ ] **Step 4: Update GetTaxonsAllOrPaged.cs:42**

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: TaxonConstant.Query.AllowedFilterFields,
    allowedSearchFields: TaxonConstant.Query.AllowedSearchFields,
    allowedSortFields: TaxonConstant.Query.AllowedSortFields);
```

- [ ] **Step 5: Update GetOptionTypesPaged.cs:28**

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: OptionTypeConstant.Query.AllowedFilterFields,
    allowedSearchFields: OptionTypeConstant.Query.AllowedSearchFields,
    allowedSortFields: OptionTypeConstant.Query.AllowedSortFields);
```

- [ ] **Step 6: Update GetOptionValuesPaged.cs:26**

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: OptionValueConstant.Query.AllowedFilterFields,
    allowedSearchFields: OptionValueConstant.Query.AllowedSearchFields,
    allowedSortFields: OptionValueConstant.Query.AllowedSortFields);
```

- [ ] **Step 7: Update ListPricesByVariant.cs:45**

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: PriceConstant.Query.AllowedFilterFields,
    allowedSearchFields: PriceConstant.Query.AllowedSearchFields,
    allowedSortFields: PriceConstant.Query.AllowedSortFields);
```

- [ ] **Step 8: Update GetAllTaxons.cs:42**

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: TaxonConstant.Query.AllowedFilterFields,
    allowedSearchFields: TaxonConstant.Query.AllowedSearchFields,
    allowedSortFields: TaxonConstant.Query.AllowedSortFields);
```

- [ ] **Step 9: Update GetAllOptionTypes.cs:31**

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: OptionTypeConstant.Query.AllowedFilterFields,
    allowedSearchFields: OptionTypeConstant.Query.AllowedSearchFields,
    allowedSortFields: OptionTypeConstant.Query.AllowedSortFields);
```

- [ ] **Step 10: Update GetProducts.cs:48 (taxon products)**

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: ProductConstant.Query.AllowedFilterFields,
    allowedSearchFields: ProductConstant.Query.AllowedSearchFields,
    allowedSortFields: ProductConstant.Query.AllowedSortFields);
```

- [ ] **Step 11: Update GetRelatedProducts.cs:67**

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: ProductConstant.Query.AllowedFilterFields,
    allowedSearchFields: ProductConstant.Query.AllowedSearchFields,
    allowedSortFields: ProductConstant.Query.AllowedSortFields);
```

- [ ] **Step 12: Update ListProducts.cs:51-54 (Storefront — replace inline with Constant)**

This handler is the ONLY one that currently passes arguments. Replace custom inline `HashSet<string>` with `ProductConstant.Query`:

Read the current handler first at lines 48-56:
```bash
rg "ParseAll" service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.cs --type cs -A 5
```

Replace the inline allowed field definitions with references to `ProductConstant.Query`:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: ProductConstant.Query.AllowedFilterFields,
    allowedSearchFields: ProductConstant.Query.AllowedSearchFields,
    allowedSortFields: ProductConstant.Query.AllowedSortFields);
```

If `StorefrontProductFilterAliases.CanonicalFields` was used for filter-aliasing, keep it applied but pass `ProductConstant.Query.AllowedFilterFields` as the whitelist. Review the existing constant's filter field list and ensure it covers what the storefront needs (may need to add `Variants.Prices.Amount` to `ProductConstant.Query.AllowedSortFields` if the current `allowedSortFields: "Name","Slug","AvailableOn","CreatedAtUtc","Variants.Prices.Amount"` differs from the constant's `"Name","CreatedAtUtc","ModifiedAtUtc","AvailableOn"`).

- [ ] **Step 13: Build and verify**

Run: `dotnet build`
Expected: 0 errors, 0 warnings

- [ ] **Step 14: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/
git commit -m "feat(catalog): enforce Constant.Query whitelists in all catalog handlers"
```

---

### Task 7: Enforce Identity handlers (2)

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Admin/Users/GetPagedOrAll/GetUsersPagedOrAll.cs:26`
- Modify: `service/Api/src/Module/Identity/Features/Admin/Roles/Get/PagedOrAll/GetRolesPagedOrAll.cs:30`

**Consumes:** `UserConstant.Query` (Task 1), `RoleConstant.Query` (Task 2)

- [ ] **Step 1: Update GetUsersPagedOrAll.cs:26**

Add using:
```csharp
using Module.Identity.Domain.Users;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: UserConstant.Query.AllowedFilterFields,
    allowedSearchFields: UserConstant.Query.AllowedSearchFields,
    allowedSortFields: UserConstant.Query.AllowedSortFields);
```

- [ ] **Step 2: Update GetRolesPagedOrAll.cs:30**

Add using:
```csharp
using Module.Identity.Domain.Roles;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: RoleConstant.Query.AllowedFilterFields,
    allowedSearchFields: RoleConstant.Query.AllowedSearchFields,
    allowedSortFields: RoleConstant.Query.AllowedSortFields);
```

- [ ] **Step 3: Build and verify**

Run: `dotnet build`
Expected: 0 errors, 0 warnings

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Identity/Features/
git commit -m "feat(identity): enforce Constant.Query whitelists in User and Role handlers"
```

---

### Task 8: Enforce Inventory handlers (4)

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockMovements/Get/Paged/GetPagedStockMovements.cs:36`
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockTransfers/Get/Paged/GetStockTransferPagedOrAll.cs:21`
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockLocations/GetPaged/GetPagedStockLocations.cs:17`
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockReservations/Get/Paged/GetPagedStockReservations.cs:24`

**Consumes:** `StockMovementConstant.Query`, `StockTransferConstant.Query`, `StockLocationConstant.Query`, `StockReservationConstant.Query` (all pre-existing)

- [ ] **Step 1: Update GetPagedStockMovements.cs:36**

Add using:
```csharp
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: StockMovementConstant.Query.AllowedFilterFields,
    allowedSearchFields: StockMovementConstant.Query.AllowedSearchFields,
    allowedSortFields: StockMovementConstant.Query.AllowedSortFields);
```

- [ ] **Step 2: Update GetStockTransferPagedOrAll.cs:21**

Add using:
```csharp
using Module.Inventory.Domain.StockTransfers;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: StockTransferConstant.Query.AllowedFilterFields,
    allowedSearchFields: StockTransferConstant.Query.AllowedSearchFields,
    allowedSortFields: StockTransferConstant.Query.AllowedSortFields);
```

- [ ] **Step 3: Update GetPagedStockLocations.cs:17**

Add using:
```csharp
using Module.Inventory.Domain.StockLocations;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: StockLocationConstant.Query.AllowedFilterFields,
    allowedSearchFields: StockLocationConstant.Query.AllowedSearchFields,
    allowedSortFields: StockLocationConstant.Query.AllowedSortFields);
```

- [ ] **Step 4: Update GetPagedStockReservations.cs:24**

Add using:
```csharp
using Module.Inventory.Domain.StockReservations;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: StockReservationConstant.Query.AllowedFilterFields,
    allowedSearchFields: StockReservationConstant.Query.AllowedSearchFields,
    allowedSortFields: StockReservationConstant.Query.AllowedSortFields);
```

Note: `StockReservationConstant.Query.AllowedSearchFields` is NOT defined. You must either:
- Add `AllowedSearchFields = []` (empty array) to the constant, OR
- Pass the empty array inline: `allowedSearchFields: []` (and add it to the constant as follow-up)

Choose option 1 — add the empty array to the constant now.

Also `StockTransferConstant.Query.AllowedSearchFields` is defined as empty `[]` — pass it directly.

- [ ] **Step 5: Build and verify**

Run: `dotnet build`
Expected: 0 errors, 0 warnings

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/ service/Api/src/Module/Inventory/Domain/
git commit -m "feat(inventory): enforce Constant.Query whitelists in all inventory handlers"
```

---

### Task 9: Enforce Location handlers (4) + consumer ref updates

**Files:**
- Modify: `service/Api/src/Module/Location/Features/Admin/Countries/GetPagedOrAll/GetCountryPagedOrAll.cs:25`
- Modify: `service/Api/src/Module/Location/Features/Admin/States/GetPagedOrAll/GetStatePagedOrAll.cs:25`
- Modify: `service/Api/src/Module/Location/Features/Store/Countries/GetPagedOrAll/GetStorefrontCountryPagedOrAll.cs:25`
- Modify: `service/Api/src/Module/Location/Features/Store/States/GetPagedOrAll/GetStorefrontStatePagedOrAll.cs:25`

**Consumes:** `CountryConstant.Query` (Task 4), `StateConstant.Query` (Task 5)

Note: The consumer ref update for Task 4 and Task 5 already updated any references from `CountryConstant.Constraints.Query` → `CountryConstant.Query` and `StateConstant.AllowedXn` → `StateConstant.Query.AllowedXn`. This task just adds the arguments.

- [ ] **Step 1: Check that consumer refs are already updated**

Run: `rg "CountryConstant.Constraints.Query\|StateConstant.Allowed" service/Api/src/ --type cs`
Expected: No matches (Task 4/5 already migrated them).

- [ ] **Step 2: Update GetCountryPagedOrAll.cs:25**

Add using:
```csharp
using Module.Location.Domain.Countries;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: CountryConstant.Query.AllowedFilterFields,
    allowedSearchFields: CountryConstant.Query.AllowedSearchFields,
    allowedSortFields: CountryConstant.Query.AllowedSortFields);
```

- [ ] **Step 3: Update GetStatePagedOrAll.cs:25**

Add using:
```csharp
using Module.Location.Domain.States;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: StateConstant.Query.AllowedFilterFields,
    allowedSearchFields: StateConstant.Query.AllowedSearchFields,
    allowedSortFields: StateConstant.Query.AllowedSortFields);
```

- [ ] **Step 4: Update GetStorefrontCountryPagedOrAll.cs:25**

Same pattern as Step 2.

- [ ] **Step 5: Update GetStorefrontStatePagedOrAll.cs:25**

Same pattern as Step 3.

- [ ] **Step 6: Build and verify**

Run: `dotnet build`
Expected: 0 errors, 0 warnings

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Location/Features/
git commit -m "feat(location): enforce Constant.Query whitelists in all location handlers"
```

---

### Task 10: Enforce Ordering handlers (3)

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Get/Paged/GetPagedOrders.cs:22`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Get/LineItems/GetOrderLineItems.cs:22`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Orders/ListOrders/ListCustomerOrders.cs:29`

**Consumes:** `OrderConstant.Query`, `LineItemConstant.Query` (pre-existing)

- [ ] **Step 1: Update GetPagedOrders.cs:22**

Add using:
```csharp
using Module.Ordering.Domain.Orders;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: OrderConstant.Query.AllowedFilterFields,
    allowedSearchFields: OrderConstant.Query.AllowedSearchFields,
    allowedSortFields: OrderConstant.Query.AllowedSortFields);
```

- [ ] **Step 2: Update GetOrderLineItems.cs:22**

Add using:
```csharp
using Module.Ordering.Domain.LineItems;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: LineItemConstant.Query.AllowedFilterFields,
    allowedSearchFields: LineItemConstant.Query.AllowedSearchFields,
    allowedSortFields: LineItemConstant.Query.AllowedSortFields);
```

Note: `LineItemConstant.Query.AllowedSearchFields` is NOT defined. Add `AllowedSearchFields = []` to the constant.

- [ ] **Step 3: Update ListCustomerOrders.cs:29**

Same pattern as Step 1 — same `OrderConstant.Query`.

- [ ] **Step 4: Build and verify**

Run: `dotnet build`
Expected: 0 errors, 0 warnings

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/ service/Api/src/Module/Ordering/Domain/
git commit -m "feat(ordering): enforce Constant.Query whitelists in all ordering handlers"
```

---

### Task 11: Enforce Payment handlers (3)

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Admin/Payments/Get/Paged/GetPagedPayments.cs:17`
- Modify: `service/Api/src/Module/Payment/Features/Admin/PaymentMethods/Get/Paged/GetPagedPaymentMethods.cs:21`
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Methods/ListPaymentMethods.cs:15`

**Consumes:** `PaymentConstant.Query`, `PaymentMethodConstant.Query` (pre-existing)

- [ ] **Step 1: Update GetPagedPayments.cs:17**

Add using:
```csharp
using Module.Payment.Domain.PaymentCaptures;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: PaymentConstant.Query.AllowedFilterFields,
    allowedSearchFields: PaymentConstant.Query.AllowedSearchFields,
    allowedSortFields: PaymentConstant.Query.AllowedSortFields);
```

- [ ] **Step 2: Update GetPagedPaymentMethods.cs:21**

Add using:
```csharp
using Module.Payment.Domain.PaymentMethods;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: PaymentMethodConstant.Query.AllowedFilterFields,
    allowedSearchFields: PaymentMethodConstant.Query.AllowedSearchFields,
    allowedSortFields: PaymentMethodConstant.Query.AllowedSortFields);
```

- [ ] **Step 3: Update ListPaymentMethods.cs:15**

Same pattern as Step 2 — same `PaymentMethodConstant.Query`.

- [ ] **Step 4: Build and verify**

Run: `dotnet build`
Expected: 0 errors, 0 warnings

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Features/
git commit -m "feat(payment): enforce Constant.Query whitelists in all payment handlers"
```

---

### Task 12: Enforce Profile handler (1)

**Files:**
- Modify: `service/Api/src/Module/Profile/Features/Store/Profiles/Get/PagedOrAll/GetProfilesPagedOrAll.cs:25`

**Consumes:** `UserProfileConstant.Query` (Task 3)

- [ ] **Step 1: Update GetProfilesPagedOrAll.cs:25**

Add using:
```csharp
using Module.Profile.Domain;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: UserProfileConstant.Query.AllowedFilterFields,
    allowedSearchFields: UserProfileConstant.Query.AllowedSearchFields,
    allowedSortFields: UserProfileConstant.Query.AllowedSortFields);
```

- [ ] **Step 2: Build and verify**

Run: `dotnet build`
Expected: 0 errors, 0 warnings

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Profile/Features/
git commit -m "feat(profile): enforce Constant.Query whitelists in profile handler"
```

---

### Task 13: Enforce Shipping handlers (3)

**Files:**
- Modify: `service/Api/src/Module/Shipping/Features/Admin/ShippingRates/Get/Paged/GetPagedShippingRates.cs:21`
- Modify: `service/Api/src/Module/Shipping/Features/Admin/ShippingMethods/Get/Paged/GetPagedShippingMethods.cs:21`
- Modify: `service/Api/src/Module/Shipping/Features/Storefront/Shipping/Rates/ListShippingRates.cs:19`

**Consumes:** `ShippingRateConstant.Query`, `ShippingMethodConstant.Query` (pre-existing)

- [ ] **Step 1: Update GetPagedShippingRates.cs:21**

Add using:
```csharp
using Module.Shipping.Domain.ShippingRates;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: ShippingRateConstant.Query.AllowedFilterFields,
    allowedSearchFields: ShippingRateConstant.Query.AllowedSearchFields,
    allowedSortFields: ShippingRateConstant.Query.AllowedSortFields);
```

- [ ] **Step 2: Update GetPagedShippingMethods.cs:21**

Add using:
```csharp
using Module.Shipping.Domain.ShippingMethods;
```

Change:
```csharp
var parsing = parameters.ParseAll();
```
To:
```csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: ShippingMethodConstant.Query.AllowedFilterFields,
    allowedSearchFields: ShippingMethodConstant.Query.AllowedSearchFields,
    allowedSortFields: ShippingMethodConstant.Query.AllowedSortFields);
```

- [ ] **Step 3: Update ListShippingRates.cs:19**

Same pattern as Step 1 — same `ShippingRateConstant.Query`.

- [ ] **Step 4: Build and verify**

Run: `dotnet build`
Expected: 0 errors, 0 warnings

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Shipping/Features/
git commit -m "feat(shipping): enforce Constant.Query whitelists in all shipping handlers"
```

---

### Task 14: Fix inventory URL mismatches (4 admin repository files)

**Files:**
- Modify: `app/Admin/src/features/inventories/stock-locations/repositories/location.repository.ts:9`
- Modify: `app/Admin/src/features/inventories/stock-transfers/repositories/transfer.repository.ts:9`
- Modify: `app/Admin/src/features/inventories/stock-movements/repositories/movement.repository.ts:8`
- Modify: `app/Admin/src/features/inventories/inventory-units/repositories/reservation.repository.ts:8`

- [ ] **Step 1: Fix location.repository.ts**

Change line 9:
```ts
return `${INVENTORY}/locations${sub ? `/${sub}` : ''}`
```
To:
```ts
return `${INVENTORY}/stock-locations${sub ? `/${sub}` : ''}`
```

- [ ] **Step 2: Fix transfer.repository.ts**

Change line 9:
```ts
return `${INVENTORY}/transfers${sub ? `/${sub}` : ''}`
```
To:
```ts
return `${INVENTORY}/stock-transfers${sub ? `/${sub}` : ''}`
```

- [ ] **Step 3: Fix movement.repository.ts**

Change line 8:
```ts
return `${INVENTORY}/movements${sub ? `/${sub}` : ''}`
```
To:
```ts
return `${INVENTORY}/stock-movements${sub ? `/${sub}` : ''}`
```

- [ ] **Step 4: Fix reservation.repository.ts**

Change line 8:
```ts
return `${INVENTORY}/reservations${sub ? `/${sub}` : ''}`
```
To:
```ts
return `${INVENTORY}/stock-reservations${sub ? `/${sub}` : ''}`
```

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/inventories/
git commit -m "fix(admin): correct inventory URL prefixes (stock- mismatch)"
```

---

### Task 15: Fix product default sort and remove DTO-only sortable columns

**Files:**
- Modify: `app/Admin/src/features/catalog/products/stores/product.store.ts:35`
- Modify: `app/Admin/src/features/catalog/products/views/ProductList.View.vue:82,242,251` (SKU filter + SKU/price sortable)
- Modify: `app/Admin/src/features/users/views/AdminUserList.View.vue:149` (fullName sortable)
- Modify: `app/Admin/src/features/catalog/taxonomies/views/TaxonomyList.View.vue:193` (taxonsCount sortable)
- Modify: `app/Admin/src/features/inventories/views/StockItemList.View.vue:165,171,186,192` (variant_name, stock_location_name, quantityReserved, countAvailable sortable)

- [ ] **Step 1: Fix default sort in product.store.ts:35**

Change:
```ts
sort: ["-created_at"],
```
To:
```ts
sort: ["-createdAtUtc"],
```

- [ ] **Step 2: Remove SKU column filter from ProductList.View.vue**

Remove these lines (~80-83):
```ts
if (skuFilter.constraints[0]?.value) {
    builder.where('Sku', '*', skuFilter.constraints[0].value);
}
```

Remove the `sku` filter definition from the `filters` ref (~39-43):
```ts
sku: {
    operator: PrimeFilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }],
},
```

Remove the `sku` filter reset from `clearFilters` (~101-104).

- [ ] **Step 3: Remove `sortable` from SKU and price columns in ProductList.View.vue**

Column `field="sku"` (line 242): remove `sortable`:
```html
<Column field="sku" :header="t('catalog.products.table.sku')" filter>
```

Column `field="price"` (line 251): remove `sortable`:
```html
<Column field="price" :header="t('catalog.products.table.price')">
```

- [ ] **Step 4: Remove `sortable` from fullName column in AdminUserList.View.vue**

Line 149: change `sortable` to just remove the attribute:
```html
<Column field="fullName" :header="t('users.table.user')">
```

- [ ] **Step 5: Remove `sortable` from taxonsCount column in TaxonomyList.View.vue**

Line 193:
```html
<Column field="taxonsCount" :header="t('catalog.taxonomies.table.taxons')" class="text-center">
```

- [ ] **Step 6: Remove `sortable` from 4 DTO-only columns in StockItemList.View.vue**

Line 165 — `field="variant_name"`: remove `sortable`:
```html
<Column field="variant_name" :header="t('inventory.table.product')">
```

Line 171 — `field="stock_location_name"`: remove `sortable`:
```html
<Column field="stock_location_name" :header="t('inventory.table.location')">
```

Line 186 — `field="quantityReserved"`: remove `sortable`:
```html
<Column field="quantityReserved" :header="t('inventory.table.reserved')" class="text-center">
```

Line 192 — `field="countAvailable"`: remove `sortable`:
```html
<Column field="countAvailable" :header="t('inventory.table.available')" class="text-center">
```

Keep `sortable` on `field="sku"` and `field="countOnHand"` — these have entity properties (`Sku` on Variant via navigation, `CountOnHand` directly on StockItem).

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/features/catalog/products/ app/Admin/src/features/users/ app/Admin/src/features/catalog/taxonomies/ app/Admin/src/features/inventories/
git commit -m "fix(admin): fix default sort and remove DTO-only sortable columns"
```

---

### Task 16: Add `searchFields` scoping to all list views

**Files:**
- Modify: `app/Admin/src/features/catalog/products/views/ProductList.View.vue` (onFilter handler)
- Modify: `app/Admin/src/features/users/views/AdminUserList.View.vue` (onFilter handler)
- Modify: `app/Admin/src/features/users/views/CustomerList.View.vue` (onFilter handler)
- Modify: `app/Admin/src/features/ordering/views/OrderList.View.vue` (onFilter handler)
- Modify: `app/Admin/src/features/catalog/taxonomies/views/TaxonomyList.View.vue` (onFilter handler)
- Modify: `app/Admin/src/features/catalog/option-types/views/OptionTypeList.View.vue` (onFilter handler — fix Description)

- [ ] **Step 1: Add `searchFields` to ProductList.View.vue `onFilter`**

In the `onFilter` function, change:
```ts
store.fetchProducts({
    search: globalFilter.value || undefined,
    filter: built.filter,
    page: 1,
});
```
To:
```ts
store.fetchProducts({
    search: globalFilter.value || undefined,
    searchFields: globalFilter.value ? ['Name', 'Description', 'Slug', 'StyleCode'] : undefined,
    filter: built.filter,
    page: 1,
});
```

- [ ] **Step 2: Add `searchFields` to AdminUserList.View.vue `onFilter`**

In the `onFilter` function, change:
```ts
store.fetchAdmins({
    search: globalFilter.value || undefined,
    page: 1,
});
```
To:
```ts
store.fetchAdmins({
    search: globalFilter.value || undefined,
    searchFields: globalFilter.value ? ['UserName', 'Email', 'FirstName', 'LastName'] : undefined,
    page: 1,
});
```

- [ ] **Step 3: Add `searchFields` to CustomerList.View.vue `onFilter`**

Find the `onFilter` handler in CustomerList.View.vue. Add the same `searchFields` pattern:
```ts
searchFields: globalFilter.value ? ['UserName', 'Email', 'FirstName', 'LastName'] : undefined,
```

- [ ] **Step 4: Add `searchFields` to OrderList.View.vue `onFilter`**

In the `onFilter` function, change:
```ts
store.fetchOrders({
    search: globalFilter.value || undefined,
    filter: built.filter,
    page: 1,
});
```
To:
```ts
store.fetchOrders({
    search: globalFilter.value || undefined,
    searchFields: globalFilter.value ? ['Number', 'Email'] : undefined,
    filter: built.filter,
    page: 1,
});
```

- [ ] **Step 5: Add `searchFields` to TaxonomyList.View.vue `onFilter`**

In the `onFilter` function, change:
```ts
store.fetchTaxonomies({
    search: globalFilter.value || undefined,
    filter: built.filter,
    page: 1,
});
```
To:
```ts
store.fetchTaxonomies({
    search: globalFilter.value || undefined,
    searchFields: globalFilter.value ? ['Name', 'Presentation'] : undefined,
    filter: built.filter,
    page: 1,
});
```

- [ ] **Step 6: Fix OptionTypeList.View.vue — remove `Description` from searchFields**

In `OptionTypeList.View.vue` `onFilter`, change:
```ts
searchFields: globalFilter.value ? ['Name', 'Presentation', 'Description'] : undefined,
```
To:
```ts
searchFields: globalFilter.value ? ['Name', 'Presentation'] : undefined,
```

- [ ] **Step 7: Remove global search input from StockItemList.View.vue**

In `StockItemList.View.vue`, remove the global search `<InputText>` and its wrapping `<IconField>` from the header template (lines 121-129). Also remove the `global` entry from the `filters` ref. The API has `AllowedSearchFields = []` — search doesn't work on this entity.

- [ ] **Step 8: Commit**

```bash
git add app/Admin/src/features/catalog/ app/Admin/src/features/users/ app/Admin/src/features/ordering/ app/Admin/src/features/inventories/
git commit -m "feat(admin): add searchFields scoping to all list views"
```

---

### Task 17: Clean up dead query parameter fields

**Files:**
- Modify: `app/Admin/src/features/ordering/types/Order.Query.Type.ts`
- Modify: `app/Admin/src/features/inventories/types/InventoryUnit.Query.Type.ts`
- Modify: `app/Admin/src/features/inventories/types/StockMovement.Query.Type.ts`
- Modify: `app/Admin/src/features/reports/types/Report.Query.Type.ts`
- Modify: `app/Admin/src/features/users/types/User.Query.Type.ts`

- [ ] **Step 1: Clean Order.Query.Type.ts**

Remove `storeId`, `warehouseId`, `fromDate`, `toDate` from the interface:

Before:
```ts
export interface OrderQuery extends ServerQueryingParameters {
    state?: string; storeId?: string; warehouseId?: string
    fromDate?: string; toDate?: string
}
```
After:
```ts
export interface OrderQuery extends ServerQueryingParameters {
    state?: string
}
```

- [ ] **Step 2: Clean InventoryUnit.Query.Type.ts**

Remove all extra fields:
```ts
export interface InventoryUnitQuery extends ServerQueryingParameters {}
```

- [ ] **Step 3: Clean StockMovement.Query.Type.ts**

Remove all extra fields:
```ts
export interface StockMovementQuery extends ServerQueryingParameters {}
```

- [ ] **Step 4: Clean Report.Query.Type.ts**

Remove `from` and `to`:
```ts
export interface DashboardQuery extends ServerQueryingParameters {}
```

- [ ] **Step 5: Clean User.Query.Type.ts**

Remove `isActive` (keep `role` — used by customer listing):
```ts
export interface UserQuery extends ServerQueryingParameters {
    role?: string
}
```

- [ ] **Step 6: Verify type-check**

Run: `pnpm run type-check` (in `app/Admin`)
Expected: 0 errors. If any consumer references the removed fields, fix those references.

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/features/
git commit -m "refactor(admin): remove dead query parameter fields from Query.Type files"
```

---

### Task 18: C# Build and Test Verification

- [ ] **Step 1: Build**

```bash
dotnet build
```
Expected: 0 errors, 0 warnings.

- [ ] **Step 2: Run unit tests (fast, no Docker)**

```bash
dotnet test service/Api/tests/Module.UnitTests
dotnet test service/Api/tests/Shared.UnitTests
```
Expected: All tests pass. If any tests fail because they send sort/filter fields not in the new whitelists, fix the test to use an allowed field.

- [ ] **Step 3: Update CONVENTIONS.md**

Add this convention to `docs/codebase/CONVENTIONS.md` under the "Query Handlers" section (or create one):

```markdown
### Query Handler Field Enforcement

All paged-query handlers MUST pass their entity's `Constant.Query` allowed fields to `parameters.ParseAll()`:

\`\`\`csharp
var parsing = parameters.ParseAll(
    allowedFilterFields: StockItemConstant.Query.AllowedFilterFields,
    allowedSearchFields: StockItemConstant.Query.AllowedSearchFields,
    allowedSortFields: StockItemConstant.Query.AllowedSortFields);
\`\`\`

This enables strict validation — invalid field names return a `Result` error (HTTP 400).
Never call `ParseAll()` without arguments in a production handler.
\```

```bash
git add docs/codebase/CONVENTIONS.md
git commit -m "docs: add query handler field enforcement convention"
```

- [ ] **Step 4: Full test suite (including integration — requires Docker)**

```bash
dotnet test
```
Expected: All tests pass.

---

### Task 19: Admin Build and Test Verification

- [ ] **Step 1: Type-check**

```bash
cd app/Admin && pnpm run type-check
```
Expected: 0 errors

- [ ] **Step 2: Lint**

```bash
cd app/Admin && pnpm run lint
```
Expected: 0 errors, 0 warnings (excluding pre-existing)

- [ ] **Step 3: Unit tests**

```bash
cd app/Admin && pnpm run test:unit
```
Expected: 161 pass, 0 fail

- [ ] **Step 4: Commit any final fixes**

```bash
git add -A && git commit -m "chore: final verification fixes for query parameter alignment"
```
