# Dead Code Cleanup Design Spec

## Summary

Remove 13 unused exports, dead state, and dead functions from the Storefront SPA. Zero behavior change — purely subtractive.

## Findings to Address

### Store State

| File | Field | Action |
|------|-------|--------|
| `features/catalog/stores/catalogStore.ts:11` | `sortOrder` | Remove ref + return. Sort handled by `usePagedQuery`. |
| `features/ordering/stores/cartStore.ts:17` | `currency` | Remove ref + return. Currency from preference system. |
| `features/ordering/stores/checkoutStore.ts:12` | `shippingMethodId` | Remove ref + return. Step components keep local refs. |
| `features/ordering/stores/checkoutStore.ts:18` | `currency` | Remove ref + return. Used internally but never read by UI. |

### Dead Components

| File | Action |
|------|--------|
| `features/catalog/components/SimilarityBadge.vue` | Delete. `VisualSearchView` uses inline styling. |

### Dead Utilities

| File | Function | Action |
|------|----------|--------|
| `shared/utils/imageUrl.ts` | `getImageUrl()` | Delete file. Image URLs used as-is from API. |
| `shared/utils/date.ts:1` | `formatDate()` | Remove function. Only `formatDateTimeUtc` used. |
| `app/composables/useCurrency.ts` | `useCurrency` | Delete file. Direct `formatCurrency` import used. |

### Dead Exports

| File | Export | Action |
|------|--------|--------|
| `features/identity/validations/login.ts:8` | `LoginFormValues` | Remove export. LoginView uses inline assertion. |
| `features/identity/validations/register.ts:15` | `RegisterFormValues` | Remove export. RegisterView uses inline assertion. |
| `shared/constants/storage.ts:5` | `STORAGE_KEYS.USER` | Remove constant. Auth managed by store. |

### Dead API Functions

| File | Function | Action |
|------|----------|--------|
| `features/shipping/services/shippingApi.ts:18` | `calculateShipping()` | Remove. Checkout uses different path. |

### Keep (Used in Tests)

| File | Export | Keep Reason |
|------|--------|-------------|
| `shared/types/result.ts:92-169` | Result factories | Used in unit tests |

## Verification

- [ ] No TypeScript errors after changes
- [ ] No lint warnings
- [ ] All 257 unit tests pass
- [ ] No visual regressions
