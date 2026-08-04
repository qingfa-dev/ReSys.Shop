# Storefront Screenshot Capture Manifest

**Harvested:** 2026-08-04, Task 7.6 (Phase 7 — thesis alignment)
**Environment:** Store SPA (`http://localhost:5174`) → .NET API (`http://localhost:5035`), Postgres pgvector + Redis (podman), Embedding service (`:8000`). Seed data (23 seeders). Puppeteer 25 / Chrome 150, viewport 1440x900.

All figures are 1440x900 PNG. Status: **final** = accurate capture of a reachable UI state; **placeholder** = shows the real UI but is constrained by a backend gap noted inline.

## Captured figures

| # | File | Shows | Status |
|---|---|---|---|
| 1 | `storefront-home.png` | Homepage hero + "New Arrivals" grid (8 product cards) | final |
| 2 | `storefront-catalog-grid.png` | Shop page: left sidebar (category tree, now fixed per 7.5a), product grid, paginator, option-type filters | final |
| 3 | `storefront-product-detail.png` | Product detail: name, price, quantity, wired "Add to Cart" button. Gallery shows the placeholder icon because the backend returns `images: []` (see gaps) | placeholder |
| 4 | `storefront-login.png` | Login form (credential + password + Sign In) | final |
| 5 | `storefront-register.png` | Registration form | final |
| 6 | `storefront-cart-empty.png` | Empty cart state (guest) | final |
| 7 | `storefront-cart.png` | Cart with 1 line item (added via the wired Add to Cart button) + order summary panel. Field mapping fixed since Task 7.5 | final |
| 8 | `storefront-checkout-address.png` | Checkout: 5-step stepper + step 1 "Shipping Address" (placeholder addresses). Reachable; steps 2–5 blocked (see gaps) | final (step 1) |
| 9 | `storefront-order-history.png` | Orders list: seeded order cards (`#DRAFT-…`, Placed) | final |
| 10 | `storefront-order-detail.png` | Order detail: PrimeVue Order Timeline (3 events), order summary + details blocks | final |
| 11 | `storefront-profile.png` | Profile view | final |
| 12 | `storefront-profile-addresses.png` | Address book | final |
| 13 | `storefront-profile-wishlists.png` | Wishlists | final |
| 14 | `storefront-profile-notifications.png` | Notification preferences | final |
| 15 | `cbir-empty-state.png` | Visual search dropzone (empty state) | final |
| 16 | `cbir-upload-state.png` | Upload state: image preview + "Search Similar Products" | final |
| 17 | `cbir-loading-state.png` | Loading state: 8 skeleton cards. (Captured by holding the search request in flight via Puppeteer request interception — harness only, no code change) | final |
| 18 | `cbir-search-error.png` | Backend-blocked state: upload UI + error message "An unhandled exception occurred while processing Command." (search-by-image request times out in the inference pipeline) | placeholder |

## Blocked figures (backend gaps — not capturable against the live stack)

| Figure | What it would show | Why blocked |
|---|---|---|
| `cbir-results-grid.png` | Visual search results grid (ranked product cards + similarity badges) | `POST /api/storefront/search-by-image` fails server-side (inference service timeout → "An unhandled exception occurred while processing Command."). Root causes documented in Task 7.5 §3.6: default model (`openclip-vit-b-32`) has no local weights, seeded embeddings use `fashion-clip`, `VectorSearchService.NpgsqlSearchAsync` SQL placeholder bug. |
| `cbir-results-empty.png` | "No similar products found" empty-results state | The backend returns a server error, never an empty result set, so the results-empty UI branch is unreachable. |
| `storefront-checkout-delivery.png` | Checkout step 2 (Delivery) | `UpdateCheckout.Handle` calls `cart.AdvanceCheckoutState(CheckoutState.Address)` (`service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.cs:81`). Address→Address is not a valid transition (`Order.Method.Checkout.cs:100-113`), so `PUT /api/storefront/cart` always returns `Order.CheckoutState.InvalidTransition` → "Cannot transition from Address to Address". Checkout cannot advance past step 1. |
| `storefront-checkout-payment.png` | Checkout step 3 (Payment) | Same cause as above. |
| `storefront-checkout-confirm.png` | Checkout step 4 (Confirm) | Same cause + the step-4 gate (`/cart/validate`) requires a completed checkout context that is unreachable. |
| `storefront-checkout-complete.png` | Checkout step 5 (Complete) | Same cause + dev has no Stripe keys (`stripe.Enabled=false`; only `bogus` provider), so `create-intent`/`place-order` cannot complete. |
| `storefront-payment-methods.png` | Payment method selection | Checkout blocked (see above). |
| `storefront-payment-stripe.png` | Stripe Elements embedded form | No Stripe keys in dev (`VITE_STRIPE_PUBLISHABLE_KEY` empty). |

## Notes

- **Add-to-cart & cart mapping** are now wired and functional (fixed since Task 7.5) — captured as a real flow: login → product page → Add to Cart → cart view shows the line item.
- **Product gallery** is empty on every product because the storefront detail mapping never assigns top-level `Images` (`ProductStore.Mapping.cs:11-32`, Task 7.5 §3.4). This is a backend gap, not a storefront defect.
- **Checkout steps 2–5** are frontend-complete (components render with placeholder data) but unreachable in a real user flow due to the `UpdateCheckout` state-transition bug above.
- The thesis chapter `04-implementations/05-frontend-ux/frontend-ux.typ` references most of these figure names (e.g. `cbir-empty-state`, `storefront-catalog-grid`, `storefront-checkout-*`). The capture names match that convention so the `// [SCREENSHOT: …]` markers can be wired in directly.
- Harness scripts (not committed): `/tmp/resys-e2e/capture-all.js`, `capture-fix.js`; browser automation log `/tmp/resys-api.log`.
