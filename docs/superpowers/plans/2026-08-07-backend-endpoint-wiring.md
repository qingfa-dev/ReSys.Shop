# Backend Endpoint Wiring Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Wire existing backend endpoints + create new default address endpoint for checkout improvements.

**Architecture:** New `GetDefaultAddress` endpoint (backend). Frontend wiring: auto-select default address in checkout, show delivery estimate from existing rates response, save card checkbox using existing setup-intent endpoint.

**Tech Stack:** .NET 10 Carter, MediatR, EF Core, Vue 3, PrimeVue

## Global Constraints

- Warnings-as-errors: `TreatWarningsAsErrors=true` in .csproj
- Vertical slice: each feature in `Features/{Admin|Storefront}/{Feature}/{Action}/` with Handler, Request, Response, Endpoint, Validator
- Result objects: all operations return `Result<T>`, not exceptions
- All storefront endpoints require `.RequireAuthorization()`
- Frontend API calls return `Result<T>` — check `.isSuccess` before use

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `service/Api/src/Module/Profile/Features/Storefront/Addresses/GetDefault/GetDefaultAddress.cs` | CREATE | Handler |
| `service/Api/src/Module/Profile/Features/Storefront/Addresses/GetDefault/GetDefaultAddress.Endpoint.cs` | CREATE | Carter endpoint |
| `service/Api/src/Module/Profile/Features/Storefront/Addresses/GetDefault/GetDefaultAddress.Response.cs` | CREATE | Response DTO |
| `app/Store/src/features/profile/services/addressApi.ts` | MODIFY | Add getDefaultAddress function |
| `app/Store/src/features/ordering/components/CheckoutStepAddress.vue` | MODIFY | Auto-select default |
| `app/Store/src/features/ordering/components/CheckoutStepPayment.vue` | MODIFY | Save card checkbox |
| `app/Store/src/features/ordering/components/CheckoutStepDelivery.vue` | MODIFY | Show delivery estimate |

---

## Tasks

### Task 1: Create GetDefaultAddress handler

**Files:**
- Create: `service/Api/src/Module/Profile/Features/Storefront/Addresses/GetDefault/GetDefaultAddress.cs`

**Interfaces:**
- Consumes: `ApplicationDbContext`, user's addresses
- Produces: `Result<Response>` with address data

- [ ] **Step 1: Read existing GetAddresses handler**

Read `service/Api/src/Module/Profile/Features/Storefront/Addresses/Get/PagedOrAll/GetAddresses.cs` for pattern reference.

- [ ] **Step 2: Create response DTO**

Create `service/Api/src/Module/Profile/Features/Storefront/Addresses/GetDefault/GetDefaultAddress.Response.cs`:

```csharp
namespace Module.Profile.Features.Storefront.Addresses.GetDefault;

public static partial class GetDefaultAddress
{
    public sealed record Response
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = "";
        public string LastName { get; init; } = "";
        public string AddressLine1 { get; init; } = "";
        public string? AddressLine2 { get; init; }
        public string City { get; init; } = "";
        public string? State { get; init; }
        public string PostalCode { get; init; } = "";
        public string Country { get; init; } = "";
        public string? Phone { get; init; }
        public bool IsDefault { get; init; }
    }
}
```

- [ ] **Step 3: Create handler**

Create `service/Api/src/Module/Profile/Features/Storefront/Addresses/GetDefault/GetDefaultAddress.cs`:

```csharp
namespace Module.Profile.Features.Storefront.Addresses.GetDefault;

public static partial class GetDefaultAddress
{
    public sealed record Query : IRequest<Result<Response>>;

    internal sealed class Handler(ApplicationDbContext db) : IRequestHandler<Query, Result<Response>>
    {
        public async ValueTask<Result<Response>> Handle(Query query, CancellationToken ct)
        {
            var userId = /* get from claims */;

            var address = await db.Addresses
                .Where(a => a.UserProfile.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (address is null)
                return Result<Response>.NotFound("No addresses found");

            return new Response
            {
                Id = address.Id,
                FirstName = address.FirstName,
                LastName = address.LastName,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country,
                Phone = address.Phone,
                IsDefault = address.IsDefault,
            };
        }
    }
}
```

- [ ] **Step 4: Create endpoint**

Create `service/Api/src/Module/Profile/Features/Storefront/Addresses/GetDefault/GetDefaultAddress.Endpoint.cs`:

```csharp
namespace Module.Profile.Features.Storefront.Addresses.GetDefault;

public static partial class GetDefaultAddress
{
    public static void MapEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet(ProfileFeature.Storefront.Addresses.GetDefault.Route, async (
            ISender sender) =>
        {
            var result = await sender.Send(new Query());
            return result.MatchCreated();
        })
        .WithName(nameof(GetDefaultAddress))
        .WithTags(ProfileFeature.Tags.Addresses)
        .RequireAuthorization();
    }
}
```

- [ ] **Step 5: Register endpoint**

Find the existing `MapProfileStorefront` extension method and add `GetDefaultAddress.MapEndpoint(app)`.

- [ ] **Step 6: Build backend**

```bash
cd service/Api && dotnet build
```

Expected: PASS

- [ ] **Step 7: Commit**

```bash
cd service/Api && git add src/Module/Profile/Features/Storefront/Addresses/GetDefault/
git commit -m "feat(profile): add GET /api/store/profiles/addresses/default endpoint"
```

### Task 2: Wire default address in checkout

**Files:**
- Modify: `app/Store/src/features/profile/services/addressApi.ts`
- Modify: `app/Store/src/features/ordering/components/CheckoutStepAddress.vue`

**Interfaces:**
- Consumes: `getDefaultAddress()` API call
- Produces: Auto-selected address in checkout

- [ ] **Step 1: Add API function**

Edit `app/Store/src/features/profile/services/addressApi.ts`. Add:

```typescript
export async function getDefaultAddress(): Promise<Result<AddressResponse>> {
  return get('api/store/profiles/addresses/default')
}
```

- [ ] **Step 2: Read CheckoutStepAddress.vue**

Read `app/Store/src/features/ordering/components/CheckoutStepAddress.vue` to understand the address selection flow.

- [ ] **Step 3: Fetch default on mount**

In the `onMounted` hook, after fetching addresses, add:

```typescript
const defaultRes = await getDefaultAddress()
if (defaultRes.isSuccess && defaultRes.value) {
  selectedAddressId.value = defaultRes.value.id
}
```

- [ ] **Step 4: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 3: Wire setup-intent for save card

**Files:**
- Modify: `app/Store/src/features/ordering/components/CheckoutStepPayment.vue`

**Interfaces:**
- Consumes: `createSetupIntent()` from `paymentApi.ts`
- Produces: "Save card" checkbox in payment step

- [ ] **Step 1: Read CheckoutStepPayment.vue**

Read `app/Store/src/features/ordering/components/CheckoutStepPayment.vue`.

- [ ] **Step 2: Add saveCard ref**

```typescript
const saveCard = ref(false)
```

- [ ] **Step 3: Add checkbox in template**

After the Stripe card element container, before the Pay button, add:

```vue
<label class="flex items-center gap-2 text-sm text-stone-600">
  <Checkbox v-model="saveCard" :binary="true" />
  Save this card for future purchases
</label>
```

- [ ] **Step 4: Call setup-intent after payment**

In the `pay` function, after payment success and before advancing steps, add:

```typescript
if (saveCard.value && paymentMethodId) {
  await paymentApi.createSetupIntent({ paymentMethodId })
}
```

- [ ] **Step 5: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 4: Show delivery estimate in checkout

**Files:**
- Modify: `app/Store/src/features/ordering/components/CheckoutStepDelivery.vue`

**Interfaces:**
- Consumes: `deliveryRange` from existing shipping rates response
- Produces: Delivery estimate text per rate

- [ ] **Step 1: Read CheckoutStepDelivery.vue**

Read `app/Store/src/features/ordering/components/CheckoutStepDelivery.vue`.

- [ ] **Step 2: Display deliveryRange**

For each shipping rate option, add below the rate name/price:

```vue
<p v-if="rate.deliveryRange" class="text-xs text-stone-500">
  Est. delivery: {{ rate.deliveryRange }}
</p>
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 4: Build backend**

```bash
cd service/Api && dotnet build
```

Expected: PASS

- [ ] **Step 5: Run unit tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 6: Commit**

```bash
cd app/Store && git add src/features/profile/services/addressApi.ts src/features/ordering/components/CheckoutStepAddress.vue src/features/ordering/components/CheckoutStepPayment.vue src/features/ordering/components/CheckoutStepDelivery.vue
git commit -m "feat(checkout): wire default address, save card, delivery estimate"
```
