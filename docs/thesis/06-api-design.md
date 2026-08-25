# Chapter 6 — API Design

## 6.1 API Style

**Decision**: Minimal API endpoints using **Carter** (`ICarterModule`) with explicit route mapping, rather than traditional ASP.NET MVC controllers.

**Justification**:

| Aspect | Carter Minimal API | MVC Controllers |
|--------|-------------------|-----------------|
| **Boilerplate** | One file per endpoint, no `[Route]` attributes | Controller class + action method + attribute routing |
| **Discoverability** | `AddEndpoints()` scans assemblies for `ICarterModule` | Reflection-based controller discovery |
| **Cohesion with Vertical Slice** | Endpoint class lives in the same folder as handler | Controllers typically in separate `Controllers/` folder |
| **Performance** | Slightly faster routing (delegates, not action descriptors) | Negligible difference at thesis scale |

The endpoint convention uses a `static partial class` with a nested `Endpoint` class implementing `ICarterModule`:

```cs
public static partial class CreateProduct
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.Products.Create.Route, ...)
               .HasPermission(CatalogPermission.AdminProductsCreate)
               .WithTags(CatalogFeature.Admin.Products.Create.Tags)
               .WithSummary(CatalogFeature.Admin.Products.Create.Summary)
               .Produces<Response>(StatusCodes.Status201Created)
               .ProducesValidationProblem();
        }
    }
}
```

**Evidence**: `Module/Catalog/Features/Admin/Products/Create/CreateProduct.Endpoint.cs:14-32`

## 6.2 Authentication and Authorization

### 6.2.1 Authentication Model

- **Primary**: JWT Bearer tokens (`Authorization: Bearer <access_token>`)
  - Access token expiry: 15 minutes
  - Refresh token expiry: 7 days
  - Algorithm: HS256 (HMAC-SHA256)
  - Secret stored in `dotnet user-secrets` for dev; env var for production

- **Secondary**: Google OAuth 2.0 for external login
  - Returns JWT pair same as password login

- **Guest**: Cookie-based guest session (`Guest` cookie, 30-day expiry, HttpOnly, Secure, SameSite=Lax)

**Evidence**: `appsettings.json:30-57`, `Shared/Security/Authentication/Tokens/Tokens.Extensions.cs:33-88`

### 6.2.2 Authorization Model

Permission-based authorization using a custom `IAuthorizationPolicyProvider`:

1. Each module declares `*FeatureMetadata` types containing route, summary, tags, and **permission descriptor**
2. Endpoints call `.HasPermission(...)` which registers a policy name dynamically
3. `PermissionPolicyProvider` maps policy name → `PermissionRequirement`
4. `PermissionContext` enumerates all allowed permission values as a registry
5. ASP.NET Identity claims store the user's permissions; the handler checks claims against the requirement

**Permission format**: `{Domain}:{Category}:{Action}` (e.g., `catalog:products:create`, `ordering:orders:cancel`)

**Evidence**: `Shared/Security/Authorization/Policies/Permission.PolicyProvider.cs:1-31`, `Shared/Security/Authorization/Registry/PermissionContext.cs:1-60`, `Shared/Security/Authorization/Attributes/HasPermission.Attribute.Extension.cs`

## 6.3 Request / Response Models

### 6.3.1 Unified Error Envelope

All failures return a consistent JSON structure:

```json
{
  "isSuccess": false,
  "statusCode": 400,
  "errors": [
    { "code": "Slug.Unique", "message": "A product with this slug already exists." }
  ],
  "message": "One or more validation errors occurred.",
  "metadata": null
}
```

**Design rationale**: A unified envelope makes client error handling predictable. The `code` field allows clients to implement i18n by mapping codes to localized messages.

**Evidence**: `Shared/Application/Models/Results/Result.Method.cs:65-186`, `Models/Errors/Error.cs`

### 6.3.2 Sample Endpoint: Create Product (Admin)

**Request**:
```http
POST /api/admin/catalog/admin/products
Content-Type: application/json
Authorization: Bearer <token>
X-CSRF-TOKEN: <anti-forgery-token>

{
  "name": "Summer Floral Dress",
  "description": "Light cotton dress with floral print",
  "slug": "summer-floral-dress",
  "status": "Draft",
  "metaTitle": "Summer Floral Dress | ReSys",
  "metaDescription": "...",
  "styleCode": "FLR-2026-001",
  "seasonName": "Summer 2026",
  "materialComposition": "100% Cotton",
  "careInstructions": "Machine wash cold",
  "fitNotes": "True to size",
  "department": "Women",
  "genderTarget": "Female",
  "availableOn": "2026-04-01T00:00:00Z"
}
```

**Response (201 Created)**:
```json
{
  "isSuccess": true,
  "statusCode": 201,
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "name": "Summer Floral Dress",
    "slug": "summer-floral-dress",
    "masterVariantId": "b2c3d4e5-f6a7-8901-bcde-f23456789012"
  }
}
```

**Evidence**: `ApiTests/Catalog/Admin/products.http`, `CreateProduct.Response.cs`

### 6.3.3 Sample Endpoint: Search by Image (Storefront)

**Request**:
```http
POST /api/admin/catalog/storefront/search-by-image
Content-Type: multipart/form-data

image: <binary image data>
```

**Response (200 OK)**:
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "data": [
    {
      "productId": "...",
      "variantId": "...",
      "name": "Similar Floral Dress",
      "similarityScore": 0.92,
      "imageUrl": "/uploads/..."
    }
  ]
}
```

**Evidence**: `ApiTests/Catalog/Storefront/search-by-image.http`

### 6.3.4 Sample Endpoint: Checkout (Storefront)

**Request**:
```http
POST /api/admin/ordering/storefront/cart/checkout
Content-Type: application/json
Authorization: Bearer <token>

{
  "billAddressId": "...",
  "shipAddressId": "...",
  "shippingMethodId": "...",
  "paymentMethodId": "..."
}
```

**Response (201 Created)**:
```json
{
  "isSuccess": true,
  "statusCode": 201,
  "data": {
    "orderId": "...",
    "orderNumber": "ORD-2026-00042",
    "total": 129.99,
    "currency": "USD",
    "checkoutState": "Payment",
    "clientSecret": "pi_xxx_secret_yyy"
  }
}
```

**Evidence**: `ApiTests/Ordering/Cart.http`, `CreateOrderFromCart.cs`

## 6.4 OpenAPI and Documentation

The API generates OpenAPI 3.0 spec automatically via `Microsoft.AspNetCore.OpenApi`. Scalar UI serves interactive documentation at `/scalar/v1`.

**FluentValidation auto-registration**: All validators are discovered at startup and registered into the MediatR pipeline. Validation errors are automatically mapped to the error envelope.

**Evidence**: `Shared/Governance/Governance.Extension.cs:1-57`, `Directory.Packages.props:53,55`

## 6.5 HTTP Test Artifacts as Living Documentation

The `ApiTests/` directory contains 49 `.http` files that serve as:
- Executable API documentation (REST Client / JetBrains HTTP Client)
- Manual QA scripts
- Thesis evidence of endpoint coverage

**Evidence**: `ApiTests/README.md:1-30`, `ApiTests/run-all.http`

## 6.6 Evidence

- `service/Api/src/Shared/Application/Endpoints/Endpoint.Extension.cs:1-63` — Carter scanning and `ToResult()` mapping
- `service/Api/src/Shared/Governance/OpenApi/OpenApi.Extension.cs:54-62` — Scalar UI registration
- `service/Api/src/Shared/Security/Authorization/Attributes/HasPermission.Attribute.Extension.cs` — permission endpoint convention
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.Endpoint.cs` — endpoint definition
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.Request.cs` — request DTO
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.Response.cs` — response DTO
- `ApiTests/` — manual HTTP test artifacts

---

## [ASK USER] Items

11. Should this chapter include a formal OpenAPI specification document (e.g., exported `openapi.json`), or are the representative samples sufficient?
12. Are there specific API standards the examiner expects adherence to (e.g., JSON:API, OData, HAL)?
