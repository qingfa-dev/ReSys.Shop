=== Frontend Applications

The user-facing components consist of two Vue 3 single-page applications: a customer storefront (PrimeVue Aura theme) and an administration dashboard (PrimeVue Sakai theme with Chart.js 4). This section presents the implemented interfaces organized by the 26 use cases defined in Section 2.2.

==== Frontend Architecture

Both applications share Vue 3.5, TypeScript ~ 6.0, Vite 8, Pinia, and Axios. All backend communication follows a typed repository pattern mirroring the .NET `Result<T>` convention with `isSuccess`, `statusCode`, `data`, and `errors[]` fields. The `BaseRepository` wraps Axios with typed CRUD:

```typescript
export interface Result<T> {
  isSuccess: boolean
  isFailure: boolean
  statusCode: number
  data?: T
  errors?: Array<{ code: string; description: string; field?: string }>
}
```

For visual search, the repository extends the base with a multipart upload method: `async searchByImage(file: File): Promise<Result<Product[]>>` dispatching `POST /api/storefront/catalog/products/images/search` with `Content-Type: multipart/form-data`.

==== Storefront Interfaces

The customer storefront implements nine use cases covering product discovery, purchasing, and account management.

#include "f1-visual-search.typ"
#include "f2-catalog-cart.typ"
#include "f3-checkout.typ"
#include "f4-order-auth-payment.typ"
#include "f5-profile.typ"

==== Administration Interfaces

The administration dashboard implements fifteen administrative use cases using PrimeVue data-table components with server-side pagination, sorting, filtering, and form dialogs with inline validation.

#include "f6-product-management.typ"
#include "f7-order-payment.typ"
#include "f8-inventory.typ"
#include "f9-user-shipping.typ"
#include "f10-system-processes.typ"
