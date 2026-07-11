---
goal: Normalize all feature Request/Response types to extend shared base models and use shared Mappings
version: 1.0
date_created: 2026-07-11
status: Planned
tags: refactor, conventions, architecture, mappings
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Fix 8 feature areas where Response/Request types (1) don't extend shared base models defined at `Shared/Models/`, (2) construct responses inline instead of calling shared `Mapping.Model.cs` extension methods, or (3) use positional record constructors breaking the member-init convention. Wishlists area is the worst offender — zero shared infrastructure exists.

## 1. Requirements & Constraints

- **REQ-001**: Every feature `Response` must extend a shared base record defined in `{Feature}/Shared/Models/{Entity}.Model.Response.cs`. One-liner `record Response : SharedBase;` is the target.
- **REQ-002**: Every feature `Request` must extend a shared base record in `Shared/Models/{Entity}.Model.Request.cs` (or `Parameters`).
- **REQ-003**: Handler must construct responses via `entity.MapToXxx<Response>()` from `Shared/Mappings/{Entity}.Mapping.Model.cs`, never inline.
- **REQ-004**: All responses must use member-init pattern `{ Property = value }`, never positional record constructors.
- **REQ-005**: Response classes that are 1:1 duplicates of a shared model must be deleted and replaced with the shared type.
- **REQ-006**: Shared mapping stubs that return default values must be enriched or deleted.
- **PAT-001**: Follow `Catalog/Admin/Products/Get/ById/GetProductById` pattern: `Response : ProductDetailResponse;` + `entity.MapToDetail<Response>()`.
- **CON-001**: All changes must build with 0 warnings (`TreatWarningsAsErrors=true`).
- **CON-002**: No cross-module references — shared models stay within each module's `Shared/` namespace.

## 2. Implementation Steps

### Phase 1 — Wishlists: Create shared infrastructure

- GOAL-001: Create `Shared/Models` and `Shared/Mappings` directories with base response/request types and mapping extensions for the Profile Wishlists area.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `Profile/Features/Store/Wishlists/Shared/Models/Wishlist.Model.Parameters.cs` — `abstract record WishlistParameters(string Name = "", bool IsPrivate = false)` | | |
| TASK-002 | Create `Profile/Features/Store/Wishlists/Shared/Models/Wishlist.Model.Response.cs` — `record WishlistDetailResponse : WishlistParameters { Guid Id; string Token; int ItemCount; bool IsDefault; }` + `record WishlistListItemResponse : WishlistParameters { Guid Id; int ItemCount; }` | | |
| TASK-003 | Create `Profile/Features/Store/Wishlists/Shared/Models/WishedItem.Model.Response.cs` — `record WishedItemResponse { Guid Id; Guid VariantId; int Quantity; DateTimeOffset AddedAtUtc; }` | | |
| TASK-004 | Create `Profile/Features/Store/Wishlists/Shared/Models/Wishlist.Model.Request.cs` — `record WishlistRequest : WishlistParameters` | | |
| TASK-005 | Create `Profile/Features/Store/Wishlists/Shared/Mappings/Wishlist.Mapping.Model.cs` — `static partial class WishlistMapping { MapToDetail<T>, MapToListItem<T> }` mapping Wishlist→WishlistDetailResponse/WishlistListItemResponse (constraint `where T : WishlistDetailResponse, new()`). Properties: Id, Name, IsPrivate, IsDefault, Token, ItemCount (from `wishlist.WishedItems.Count`), WishedItems | | |
| TASK-006 | Create `Profile/Features/Store/Wishlists/Shared/Mappings/Wishlist.Mapping.Domain.cs` — empty partial with comment "Request-to-domain mapping handled via WishlistExtensions.Create directly." | | |

### Phase 2 — Wishlists: Update all 7 feature files

- GOAL-002: Convert every Wishlist feature's Response/Request to extend shared bases and use shared mappings.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | **GetWishlistById.Response.cs** — Replace `sealed class Response { ... }` + `sealed class WishedItemResponse { ... }` with `record Response : WishlistDetailResponse`. Delete inline `WishedItemResponse`. | | |
| TASK-008 | **GetWishlistById.cs** — Inline mapping at L34-52: replace `new Response { Id = ..., Name = ..., ... }` with `wishlist.MapToDetail<Response>()`. | | |
| TASK-009 | **GetWishlists.Response.cs** — Replace `sealed class Response { ... }` with `record Response : WishlistListItemResponse`. | | |
| TASK-010 | **GetWishlists.cs** — Inline `.Select(w => new Response { ... })` at L37-44: replace with `.Select(w => w.MapToListItem<Response>())`. | | |
| TASK-011 | **CreateWishlist.Response.cs** — Replace `sealed class Response { ... }` with `record Response : WishlistDetailResponse`. | | |
| TASK-012 | **CreateWishlist.cs** — Inline mapping at L38-46: replace `new Response { Id = ..., Name = ..., ... }` with `wishlist.MapToDetail<Response>()`. | | |
| TASK-013 | **AddWishlistItem.Response.cs** — Replace `sealed class Response { ... }` with `record Response : WishlistDetailResponse`. | | |
| TASK-014 | **AddWishlistItem.cs** — Inline mapping at L43-51: replace `new Response { ... }` with `wishlist.MapToDetail<Response>()`. | | |
| TASK-015 | **UpdateWishlist.Response.cs** — Replace `sealed class Response { ... }` with `record Response : WishlistDetailResponse`. | | |
| TASK-016 | **UpdateWishlist.cs** (needs read) — Replace inline mapping with `wishlist.MapToDetail<Response>()`. | | |
| TASK-017 | **DeleteWishlist.Response.cs** — Positional `record Response(Guid Id, string Name)` → convert to `record Response : WishlistDetailResponse`. | | |
| TASK-018 | **DeleteWishlist.cs** (needs read) — Fix inline construction. | | |
| TASK-019 | **RemoveWishlistItem.Response.cs** — Positional `record Response(Guid Id, string Name)` → convert to `record Response : WishlistDetailResponse`. | | |
| TASK-020 | **RemoveWishlistItem.cs** (needs read) — Fix inline construction. | | |
| TASK-021 | **CreateWishlist.Request.cs** — `sealed class Request { ... }` → `record Request : WishlistRequest`. | | |
| TASK-022 | **AddWishlistItem.Request.cs** — `sealed class Request { ... }` → `record Request : WishlistRequest` (or define `AddWishlistItemRequest : WishlistParameters` for VariantId+Quantity). | | |

### Phase 3 — Cart/Get

- GOAL-003: GetCart Response must extend shared `CartDetailResponse` and use `CartMapping.MapToDetail<T>`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-023 | **Cart.Mapping.Model.cs** — Replace stub `MapToDetail<T>` with real mapping: `Id`, `Items`, `ItemTotal`, `Total`, `Currency`, `ItemCount`, `CheckoutState`. Add `MapToCartItem<T>(this LineItem entity)` mapping LineItem→CartItem with variant lookup injected via param or delegate. | | |
| TASK-024 | **GetCart.Response.cs** — Replace `class Response { ... }` + `class CartItem { ... }` with `record Response : CartDetailResponse`. Delete inline `CartItem` (use shared `CartItem` from `Cart/Shared/Models`). | | |
| TASK-025 | **GetCart.cs** — Replace inline mapping at L57-79 with `cart.MapToDetail<Response>()`. Drop variant lookup into dictionary — mapping method handles it (pass `variants` dict as param to mapping). | | |

### Phase 4 — Cart/AddItem

- GOAL-004: AddToCart Request extends CartRequest, Response respects pattern.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-026 | **AddToCart.Request.cs** — `class Request { Guid VariantId; int Quantity }` → `record Request : CartRequest` (CartRequest at `Cart/Shared/Models` already has `VariantId`, `Quantity`, `Notes` — remove `Notes` override or keep it null). | | |
| TASK-027 | **AddToCart.Response.cs** — `class Response { Guid LineItemId }` → `record Response`. This is an operation-result type (not a cart view); no base model fits. Convert to `record` with member init to satisfy REQ-004. | | |
| TASK-028 | **AddToCart.cs** — Inline `new Response { LineItemId = ... }` at L84, L102-103: extract to `CartMapping.LineItemAdded<Response>(LineItem entity)` or construct directly (acceptable since it's a simple ID-only response). | | |

### Phase 5 — SyncVariantPrices

- GOAL-005: SyncVariantPrices Response extends PriceResponse or documents divergence.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-029 | **SyncVariantPrices.Response.cs** — `sealed record Response { int Added; int Updated; int Removed; }` → `record Response : PriceResponse { int Added; int Updated; int Removed; }`. `PriceResponse` (at `Prices/Shared/Models`) has `Id`, `VariantId` — set `VariantId` from command. | | |
| TASK-030 | **SyncVariantPrices.cs** — Line 115-117: replace `new Response { Added = added, ... }` with `new Response { VariantId = variantId, Added = added, ... }`. | | |

### Phase 6 — EmailRegister

- GOAL-006: Convert positional Response to member-init pattern.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-031 | **EmailRegister.Response.cs** — `record Response(Guid UserId, string Email, string Message)` → `record Response { Guid UserId; string Email; string Message; }`. No shared base exists for registration (token responses use `BaseTokenResponseModel` which doesn't fit). | | |
| TASK-032 | **EmailRegister.cs** — Line 86-89: replace `new Response(user.Id, user.Email, ...)` with `new Response { UserId = user.Id, Email = user.Email, Message = UserResult.Success.Registered }`. | | |

### Phase 7 — GetImage

- GOAL-007: Convert positional Response to member-init, fix Endpoint pattern.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-033 | **GetImage.cs** — `record Response(string FullPath, string ContentType)` → `record Response { string FullPath; string ContentType; }`. No shared base needed — this is a file-serving DTO. | | |
| TASK-034 | **GetImage.cs** — Line 43: replace `new Response(fullPath, image.ContentType)` with `new Response { FullPath = fullPath, ContentType = image.ContentType }`. | | |
| TASK-035 | **GetImage.Endpoint.cs** — L19-22 manual `IsFailure` check is acceptable here (returns `PhysicalFile` on success, not JSON). Add comment explaining divergence from `result.ToResult()` pipeline. | | |

## 3. Alternatives

- **ALT-001**: Skip Wishlists shared models and just convert classes to records. Rejected because 7 feature files all duplicate the same property sets — extraction pays for itself on first reuse.
- **ALT-002**: Make `CartMapping.MapToDetail<T>` accept a delegate for variant resolution. Rejected — the variant dict should be passed as a parameter to the mapping method instead.
- **ALT-003**: Force `EmailRegister.Response` to extend `BaseTokenResponseModel`. Rejected — registration returns `UserId + Email + Message`, not tokens. A shared `AuthResponseModel` could be created but adds abstraction without reuse.

## 4. Dependencies

- **DEP-001**: Phase 1 (Wishlists shared infra) must complete before Phase 2 (Wishlists feature updates).
- **DEP-002**: Phase 3 (Cart/Get) depends on `CartMapping.MapToDetail<T>` enrichment (TASK-023) before feature update (TASK-024, TASK-025).
- **DEP-003**: Phases 3-7 are independent of each other and can run in parallel.
- **DEP-004**: All phases must build with `dotnet build src/Api/Api.csproj` — 0 warnings, 0 errors.

## 5. Files

### Wishlists (new files)
- **FILE-001**: `Profile/Features/Store/Wishlists/Shared/Models/Wishlist.Model.Parameters.cs` — abstract base parameters
- **FILE-002**: `Profile/Features/Store/Wishlists/Shared/Models/Wishlist.Model.Response.cs` — detail + list response bases
- **FILE-003**: `Profile/Features/Store/Wishlists/Shared/Models/WishedItem.Model.Response.cs` — shared item response
- **FILE-004**: `Profile/Features/Store/Wishlists/Shared/Models/Wishlist.Model.Request.cs` — shared request base
- **FILE-005**: `Profile/Features/Store/Wishlists/Shared/Mappings/Wishlist.Mapping.Model.cs` — entity→response mapper
- **FILE-006**: `Profile/Features/Store/Wishlists/Shared/Mappings/Wishlist.Mapping.Domain.cs` — request→domain mapper (empty)

### Wishlists (modified files)
- **FILE-007**: `Profile/Features/Store/Wishlists/GetById/GetWishlistById.Response.cs` — extend `WishlistDetailResponse`
- **FILE-008**: `Profile/Features/Store/Wishlists/GetById/GetWishlistById.cs` — use `MapToDetail<Response>()`
- **FILE-009**: `Profile/Features/Store/Wishlists/Get/GetWishlists.Response.cs` — extend `WishlistListItemResponse`
- **FILE-010**: `Profile/Features/Store/Wishlists/Get/GetWishlists.cs` — use `MapToListItem<Response>()`
- **FILE-011**: `Profile/Features/Store/Wishlists/Create/CreateWishlist.Response.cs` — extend `WishlistDetailResponse`
- **FILE-012**: `Profile/Features/Store/Wishlists/Create/CreateWishlist.cs` — use `MapToDetail<Response>()`
- **FILE-013**: `Profile/Features/Store/Wishlists/Create/CreateWishlist.Request.cs` — extend `WishlistRequest`
- **FILE-014**: `Profile/Features/Store/Wishlists/AddItem/AddWishlistItem.Response.cs` — extend `WishlistDetailResponse`
- **FILE-015**: `Profile/Features/Store/Wishlists/AddItem/AddWishlistItem.cs` — use `MapToDetail<Response>()`
- **FILE-016**: `Profile/Features/Store/Wishlists/AddItem/AddWishlistItem.Request.cs` — extend shared request
- **FILE-017**: `Profile/Features/Store/Wishlists/Update/UpdateWishlist.Response.cs` — extend `WishlistDetailResponse`
- **FILE-018**: `Profile/Features/Store/Wishlists/Delete/DeleteWishlist.Response.cs` — positional→member-init, extend `WishlistDetailResponse`
- **FILE-019**: `Profile/Features/Store/Wishlists/RemoveItem/RemoveWishlistItem.Response.cs` — positional→member-init, extend `WishlistDetailResponse`

### Cart (modified files)
- **FILE-020**: `Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Model.cs` — enrich `MapToDetail<T>` with real properties
- **FILE-021**: `Ordering/Features/Storefront/Cart/Get/GetCart.Response.cs` — extend `CartDetailResponse`, delete `CartItem`
- **FILE-022**: `Ordering/Features/Storefront/Cart/Get/GetCart.cs` — use `MapToDetail<Response>()`
- **FILE-023**: `Ordering/Features/Storefront/Cart/AddItem/AddToCart.Response.cs` — `class`→`record`
- **FILE-024**: `Ordering/Features/Storefront/Cart/AddItem/AddToCart.Request.cs` — extend `CartRequest`

### Other (modified files)
- **FILE-025**: `Catalog/Admin/Products/Variants/Prices/Sync/SyncVariantPrices.Response.cs` — extend `PriceResponse`
- **FILE-026**: `Catalog/Admin/Products/Variants/Prices/Sync/SyncVariantPrices.cs` — set `VariantId` in response
- **FILE-027**: `Identity/Store/Auth/Register/EmailRegister.Response.cs` — positional→member-init
- **FILE-028**: `Identity/Store/Auth/Register/EmailRegister.cs` — member-init construction
- **FILE-029**: `Catalog/Storefront/Images/Get/Image/GetImage.cs` — positional→member-init
- **FILE-030**: `Catalog/Storefront/Images/Get/Image/GetImage.Endpoint.cs` — add divergence comment

## 6. Testing

- **TEST-001**: `dotnet build src/Api/Api.csproj` — must produce 0 warnings, 0 errors (warnings-as-errors enforced).
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` — existing tests must pass (pre-existing errors in Stripe tests acceptable).
- **TEST-003**: Run each modified feature's `.http` file from `ApiTests/` if available to verify endpoint response shape unchanged.

## 7. Risks & Assumptions

- **RISK-001**: Wishlists `MapToDetail<T>` must eagerly load `WishedItems` to compute `ItemCount` and build `WishedItems` list — existing queries already `.Include(w => w.WishedItems)`, so no perf regression.
- **RISK-002**: CartMapping.MapToDetail<T> needs variant data (name, SKU) for CartItem mapping — currently loaded via a separate dictionary query in GetCart handler. The mapping method signature must accept `Dictionary<Guid, Variant>` as parameter.
- **ASSUMPTION-001**: `PriceResponse` base at `Prices/Shared/Models` is the correct shared base for `SyncVariantPrices.Response` (it has `Id` + `VariantId` + all `PriceParameters` properties).
- **ASSUMPTION-002**: No external consumers depend on the exact JSON shape of these responses (property names unchanged, only inheritance changes).

## 8. Related Specifications / Further Reading

- `docs/codebase/CONVENTIONS.md` — coding conventions for vertical slices
- `service/Api/src/Module/Catalog/Features/Admin/Products/Get/ById/GetProductById.Response.cs` — reference pattern (one-liner `record Response : ProductDetailResponse;`)
- `service/Api/src/Module/Catalog/Features/Admin/Products/Shared/Mappings/Product.Mapping.Model.cs` — reference mapping pattern
