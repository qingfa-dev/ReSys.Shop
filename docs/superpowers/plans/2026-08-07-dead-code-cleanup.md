# Dead Code Cleanup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove 13 unused exports, dead state, and dead functions from the Storefront SPA. Zero behavior change.

**Architecture:** Subtractive only. Remove refs, functions, components, and files that are never imported or called by application code. Keep test-only utilities.

**Tech Stack:** Vue 3, Pinia, TypeScript, PrimeVue 5

## Global Constraints

- Warnings-as-errors: any TypeScript/lint warning fails build
- Do NOT remove `shared/types/result.ts` factories (used in tests)
- Verify no imports exist before deleting files
- Run `pnpm run lint` and `pnpm run test:unit` after each task

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `features/catalog/stores/catalogStore.ts` | MODIFY | Remove `sortOrder` |
| `features/ordering/stores/cartStore.ts` | MODIFY | Remove `currency` |
| `features/ordering/stores/checkoutStore.ts` | MODIFY | Remove `shippingMethodId`, `currency` |
| `features/catalog/components/SimilarityBadge.vue` | DELETE | Unused component |
| `shared/utils/imageUrl.ts` | DELETE | Unused utility |
| `shared/utils/date.ts` | MODIFY | Remove `formatDate` |
| `app/composables/useCurrency.ts` | DELETE | Unused composable |
| `features/identity/validations/login.ts` | MODIFY | Remove `LoginFormValues` |
| `features/identity/validations/register.ts` | MODIFY | Remove `RegisterFormValues` |
| `shared/constants/storage.ts` | MODIFY | Remove `STORAGE_KEYS.USER` |
| `features/shipping/services/shippingApi.ts` | MODIFY | Remove `calculateShipping` |

---

## Tasks

### Task 1: Remove dead store state

**Files:**
- Modify: `app/Store/src/features/catalog/stores/catalogStore.ts`
- Modify: `app/Store/src/features/ordering/stores/cartStore.ts`
- Modify: `app/Store/src/features/ordering/stores/checkoutStore.ts`

**Interfaces:**
- Consumes: None
- Produces: Cleaner store definitions

- [ ] **Step 1: Read catalogStore.ts**

Read `app/Store/src/features/catalog/stores/catalogStore.ts`. Find `sortOrder` ref on line 11.

- [ ] **Step 2: Remove sortOrder from catalogStore**

Delete the `sortOrder` ref (line 11) and remove it from the return object (line 49).

- [ ] **Step 3: Read cartStore.ts**

Read `app/Store/src/features/ordering/stores/cartStore.ts`. Find `currency` ref.

- [ ] **Step 4: Remove currency from cartStore**

Delete the `currency` ref and all references to it in the store. Remove from return object.

- [ ] **Step 5: Read checkoutStore.ts**

Read `app/Store/src/features/ordering/stores/checkoutStore.ts`. Find `shippingMethodId` and `currency` refs.

- [ ] **Step 6: Remove dead state from checkoutStore**

Delete `shippingMethodId` ref (line 12) and `currency` ref (line 18). Remove both from return object. Keep `currency` usage in `saveAddress` if it's used internally — check if it's passed to `updateCheckout`.

- [ ] **Step 7: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 8: Run tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 9: Commit**

```bash
cd app/Store && git add src/features/catalog/stores/catalogStore.ts src/features/ordering/stores/cartStore.ts src/features/ordering/stores/checkoutStore.ts
git commit -m "chore: remove dead store state (sortOrder, currency, shippingMethodId)"
```

### Task 2: Delete dead component and utilities

**Files:**
- Delete: `app/Store/src/features/catalog/components/SimilarityBadge.vue`
- Delete: `app/Store/src/shared/utils/imageUrl.ts`
- Delete: `app/Store/src/app/composables/useCurrency.ts`

**Interfaces:**
- Consumes: None
- Produces: None

- [ ] **Step 1: Verify SimilarityBadge not imported**

```bash
cd app/Store && grep -r "SimilarityBadge" src/ --include="*.vue" --include="*.ts"
```

Expected: No matches (or only its own file)

- [ ] **Step 2: Delete SimilarityBadge.vue**

```bash
rm app/Store/src/features/catalog/components/SimilarityBadge.vue
```

- [ ] **Step 3: Verify imageUrl.ts not imported**

```bash
cd app/Store && grep -r "getImageUrl\|imageUrl" src/ --include="*.vue" --include="*.ts" | grep -v "imageUrl.ts"
```

Expected: No matches

- [ ] **Step 4: Delete imageUrl.ts**

```bash
rm app/Store/src/shared/utils/imageUrl.ts
```

- [ ] **Step 5: Verify useCurrency.ts not imported**

```bash
cd app/Store && grep -r "useCurrency" src/ --include="*.vue" --include="*.ts" | grep -v "useCurrency.ts"
```

Expected: No matches

- [ ] **Step 6: Delete useCurrency.ts**

```bash
rm app/Store/src/app/composables/useCurrency.ts
```

- [ ] **Step 7: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 8: Run tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 9: Commit**

```bash
cd app/Store && git add -A
git commit -m "chore: delete dead SimilarityBadge, imageUrl, useCurrency"
```

### Task 3: Remove dead exports and functions

**Files:**
- Modify: `app/Store/src/shared/utils/date.ts`
- Modify: `app/Store/src/features/identity/validations/login.ts`
- Modify: `app/Store/src/features/identity/validations/register.ts`
- Modify: `app/Store/src/shared/constants/storage.ts`
- Modify: `app/Store/src/features/shipping/services/shippingApi.ts`

**Interfaces:**
- Consumes: None
- Produces: Cleaner exports

- [ ] **Step 1: Read date.ts**

Read `app/Store/src/shared/utils/date.ts`. Find `formatDate` function.

- [ ] **Step 2: Remove formatDate from date.ts**

Delete the `formatDate` function. Keep `formatDateTimeUtc`.

- [ ] **Step 3: Read login.ts validations**

Read `app/Store/src/features/identity/validations/login.ts`. Find `LoginFormValues` type export.

- [ ] **Step 4: Remove LoginFormValues export**

Delete the `export type LoginFormValues` line.

- [ ] **Step 5: Read register.ts validations**

Read `app/Store/src/features/identity/validations/register.ts`. Find `RegisterFormValues` type export.

- [ ] **Step 6: Remove RegisterFormValues export**

Delete the `export type RegisterFormValues` line.

- [ ] **Step 7: Read storage.ts constants**

Read `app/Store/src/shared/constants/storage.ts`. Find `STORAGE_KEYS.USER`.

- [ ] **Step 8: Remove STORAGE_KEYS.USER**

Delete the `USER` line from the `STORAGE_KEYS` object.

- [ ] **Step 9: Read shippingApi.ts**

Read `app/Store/src/features/shipping/services/shippingApi.ts`. Find `calculateShipping` function.

- [ ] **Step 10: Remove calculateShipping function**

Delete the `calculateShipping` function export.

- [ ] **Step 11: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 12: Run tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 13: Commit**

```bash
cd app/Store && git add -A
git commit -m "chore: remove dead exports (formatDate, LoginFormValues, RegisterFormValues, STORAGE_KEYS.USER, calculateShipping)"
```
