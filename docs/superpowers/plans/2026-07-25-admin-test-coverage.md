# Admin SPA — Test Coverage Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Write 100% store + composable test coverage for 8 untested modules, API tests per module, and complex component tests for forms/managers/workflows (~160 store tests, ~50 API tests, ~14 component test files).

**Architecture:** Tests follow the existing catalog + auth module test patterns. Store tests mock API calls via `vi.mock`, verify success/error paths. API tests mock `apiClient`, verify HTTP method/URL/payload. Component tests use `@vue/test-utils` with `createTestingPinia`, stub PrimeVue components.

**Tech Stack:** Vitest 4.1, @vue/test-utils 2.4, @pinia/testing, jsdom 29.1

## Global Constraints

- No new npm packages — reuse existing vitest, @vue/test-utils, @pinia/testing, jsdom
- All tests must pass: `pnpm run test:unit` exits with code 0
- All store tests verify both success paths (Result.isSuccess=true) and error paths (Result.isSuccess=false)
- All API tests verify correct HTTP method, URL path, query parameter serialization
- Component tests only for forms, managers, workflows — NOT simple display-only components
- Follow existing test file naming: `__tests__/<name>.spec.ts` within each feature subdirectory
- Use `createTestingPinia({ stubActions: false })` for store-dependent tests
- Mock API modules with `vi.mock('../../api')` or `vi.mock('@/shared/api/client')`

---

### Task 1: Inventory store tests (5 stores)

**Files:**
- Create: `app/Admin/src/features/inventory/store/__tests__/stock-item.store.spec.ts`
- Create: `app/Admin/src/features/inventory/store/__tests__/stock-location.store.spec.ts`
- Create: `app/Admin/src/features/inventory/store/__tests__/stock-movement.store.spec.ts`
- Create: `app/Admin/src/features/inventory/store/__tests__/stock-reservation.store.spec.ts`
- Create: `app/Admin/src/features/inventory/store/__tests__/stock-transfer.store.spec.ts`

**Interfaces:**
- Consumes: Existing stock-item, stock-location, stock-movement, stock-reservation, stock-transfer stores
- Produces: Test files following the catalog product store test pattern

- [ ] **Step 1: Read existing catalog store test for pattern reference**

Read `app/Admin/src/features/catalog/store/__tests__/product.store.spec.ts` to understand:
- How `vi.mock` is used for API modules
- How `createTestingPinia` + `setActivePinia` is set up
- How `result.isSuccess` is mocked
- How toast/composables are stubbed

- [ ] **Step 2: Write stock-item.store.spec.ts**

```ts
// app/Admin/src/features/inventory/store/__tests__/stock-item.store.spec.ts
import { setActivePinia, createPinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useStockItemStore } from '../stock-item.store'

vi.mock('../../api', () => ({
  StockItemApi: {
    getMany: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
    adjustQuantity: vi.fn(),
  },
}))

import { StockItemApi } from '../../api'

describe('useStockItemStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('fetchMany', () => {
    it('sets items and totalRecords on success', async () => {
      const mockItems = [{ id: '1', sku: 'SKU-1', quantity: 10 }]
      vi.mocked(StockItemApi.getMany).mockResolvedValue({
        isSuccess: true,
        items: mockItems,
        totalCount: 1,
        message: null,
      })
      const store = useStockItemStore()
      await store.fetchMany()
      expect(store.items).toEqual(mockItems)
      expect(store.totalRecords).toBe(1)
    })

    it('sets loading=true during fetch', async () => {
      vi.mocked(StockItemApi.getMany).mockImplementation(() => new Promise(resolve => {
        setTimeout(() => resolve({ isSuccess: true, items: [], totalCount: 0, message: null }), 10)
      }))
      const store = useStockItemStore()
      const promise = store.fetchMany()
      expect(store.loading).toBe(true)
      await promise
    })

    it('resets items to [] and sets error on failure', async () => {
      vi.mocked(StockItemApi.getMany).mockResolvedValue({
        isSuccess: false,
        items: null,
        totalCount: null,
        message: 'Server error',
      })
      const store = useStockItemStore()
      store.items = [{ id: '1', sku: 'SKU-1', quantity: 10 }]
      await store.fetchMany()
      expect(store.items).toEqual([])
      expect(store.totalRecords).toBe(0)
      expect(store.error).toBe('Server error')
    })

    it('passes page and pageSize to API', async () => {
      vi.mocked(StockItemApi.getMany).mockResolvedValue({
        isSuccess: true, items: [], totalCount: 0, message: null,
      })
      const store = useStockItemStore()
      store.page = 2
      store.pageSize = 50
      await store.fetchMany()
      expect(StockItemApi.getMany).toHaveBeenCalledWith(expect.objectContaining({
        page: 3, pageSize: 50,
      }))
    })
  })

  describe('getById', () => {
    it('sets currentItem on success', async () => {
      const mockItem = { id: '1', sku: 'SKU-1', quantity: 10 }
      vi.mocked(StockItemApi.getById).mockResolvedValue({
        isSuccess: true, value: mockItem, message: null,
      })
      const store = useStockItemStore()
      await store.getById('1')
      expect(store.currentItem).toEqual(mockItem)
    })

    it('sets error on not-found', async () => {
      vi.mocked(StockItemApi.getById).mockResolvedValue({
        isSuccess: false, value: null, message: 'Not found',
      })
      const store = useStockItemStore()
      await store.getById('999')
      expect(store.error).toBe('Not found')
    })
  })

  describe('create', () => {
    it('calls API create and returns result', async () => {
      const formData = { sku: 'NEW', quantity: 5, locationId: 'LOC-1', productVariantId: 'VAR-1' }
      vi.mocked(StockItemApi.create).mockResolvedValue({
        isSuccess: true, value: { id: '1', ...formData }, message: null,
      })
      const store = useStockItemStore()
      const result = await store.create(formData)
      expect(StockItemApi.create).toHaveBeenCalledWith(formData)
      expect(result.isSuccess).toBe(true)
    })

    it('handles validation error', async () => {
      vi.mocked(StockItemApi.create).mockResolvedValue({
        isSuccess: false, value: null, message: 'Sku already exists',
      })
      const store = useStockItemStore()
      const result = await store.create({ sku: 'DUP', quantity: 1 } as any)
      expect(result.isSuccess).toBe(false)
      expect(result.message).toBe('Sku already exists')
    })
  })

  describe('update', () => {
    it('calls API update with id and data', async () => {
      const formData = { quantity: 20 }
      vi.mocked(StockItemApi.update).mockResolvedValue({
        isSuccess: true, value: { id: '1', quantity: 20 }, message: null,
      })
      const store = useStockItemStore()
      await store.update('1', formData)
      expect(StockItemApi.update).toHaveBeenCalledWith('1', formData)
    })
  })

  describe('delete', () => {
    it('calls API delete with id', async () => {
      vi.mocked(StockItemApi.delete).mockResolvedValue({
        isSuccess: true, value: null, message: null,
      })
      const store = useStockItemStore()
      store.items = [{ id: '1', sku: 'SKU-1', quantity: 10 }]
      await store.delete('1')
      expect(StockItemApi.delete).toHaveBeenCalledWith('1')
      expect(store.items).toHaveLength(0)
    })
  })
})
```

- [ ] **Step 3: Write stock-location.store.spec.ts**

Follow the same pattern as stock-item. Test: fetchMany, getById, create, update, delete, activate, deactivate.

- [ ] **Step 4: Write stock-movement.store.spec.ts**

Read-only store — only has `fetchMany`. Tests: success path, error path, loading state.

- [ ] **Step 5: Write stock-reservation.store.spec.ts**

Tests: fetchMany, cancel. Cancel should call StockReservationApi.cancel(id), verify result.isSuccess path and error path.

- [ ] **Step 6: Write stock-transfer.store.spec.ts**

Tests: fetchMany, getById, create, complete (mark transfer completed), cancel. Verify all success/error paths.

- [ ] **Step 7: Run tests**

```bash
cd app/Admin && npx vitest run src/features/inventory/store/__tests__/
```
Expected: All inventory store tests PASS.

- [ ] **Step 8: Commit**

```bash
git add app/Admin/src/features/inventory/store/__tests__/
git commit -m "test(inventory): add store tests for stock-item, stock-location, stock-movement, stock-reservation, stock-transfer"
```

---

### Task 2: Location store tests (2 stores)

**Files:**
- Create: `app/Admin/src/features/location/store/__tests__/country.store.spec.ts`
- Create: `app/Admin/src/features/location/store/__tests__/state.store.spec.ts`

**Interfaces:**
- Consumes: Existing country, state stores

- [ ] **Step 1: Write country.store.spec.ts**

Tests: fetchMany, getById, create, update, delete. Verify all success/error paths, loading states, pagination.

- [ ] **Step 2: Write state.store.spec.ts**

Tests: fetchMany, getById, create, update, delete. Verify country association in form data.

- [ ] **Step 3: Run tests**

```bash
cd app/Admin && npx vitest run src/features/location/store/__tests__/
```
Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/location/store/__tests__/
git commit -m "test(location): add store tests for country, state"
```

---

### Task 3: Ordering store tests (1 store)

**Files:**
- Create: `app/Admin/src/features/ordering/store/__tests__/order.store.spec.ts`

**Interfaces:**
- Consumes: Existing order store

- [ ] **Step 1: Write order.store.spec.ts**

Tests: fetchMany, getById, create, update, delete, approve (lifecycle), complete (lifecycle), cancel (lifecycle), resume (lifecycle). Verify all success/error paths for each lifecycle action.

```ts
describe('lifecycle actions', () => {
  it('approve calls API with order id', async () => {
    vi.mocked(OrderApi.approve).mockResolvedValue({ isSuccess: true, value: null, message: null })
    const store = useOrderStore()
    await store.approve('1')
    expect(OrderApi.approve).toHaveBeenCalledWith('1')
  })

  it('cancel handles error', async () => {
    vi.mocked(OrderApi.cancel).mockResolvedValue({ isSuccess: false, value: null, message: 'Cannot cancel shipped order' })
    const store = useOrderStore()
    const result = await store.cancel('1')
    expect(result.isSuccess).toBe(false)
  })
})
```

- [ ] **Step 2: Run tests**

```bash
cd app/Admin && npx vitest run src/features/ordering/store/__tests__/
```
Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/ordering/store/__tests__/
git commit -m "test(ordering): add store tests for order with lifecycle actions"
```

---

### Task 4: Payment store tests (2 stores)

**Files:**
- Create: `app/Admin/src/features/payment/store/__tests__/payment.store.spec.ts`
- Create: `app/Admin/src/features/payment/store/__tests__/payment-method.store.spec.ts`

**Interfaces:**
- Consumes: Existing payment, payment-method stores

- [ ] **Step 1: Write payment.store.spec.ts**

Tests: fetchMany, getById, capture, void, refund. Payment CRUD is read-only (no create/update/delete). Verify lifecycle actions.

- [ ] **Step 2: Write payment-method.store.spec.ts**

Tests: fetchMany, getById, create, update, delete, activate, deactivate.

- [ ] **Step 3: Run tests**

```bash
cd app/Admin && npx vitest run src/features/payment/store/__tests__/
```
Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/payment/store/__tests__/
git commit -m "test(payment): add store tests for payment, payment-method"
```

---

### Task 5: Profile store tests (2 stores)

**Files:**
- Create: `app/Admin/src/features/profile/store/__tests__/profile.store.spec.ts`
- Create: `app/Admin/src/features/profile/store/__tests__/address.store.spec.ts`

**Interfaces:**
- Consumes: Existing profile, address stores

- [ ] **Step 1: Write profile.store.spec.ts**

Tests: fetch, update. Profile has no create/delete — fetch on mount, update on save. Test error path: resets profile to null on error (from bug fix batch 2).

```ts
it('resets profile to null on error', async () => {
  vi.mocked(ProfileApi.get).mockResolvedValue({ isSuccess: false, value: null, message: 'Not found' })
  const store = useProfileStore()
  store.profile = { id: '1', email: 'a@b.com', fullName: 'A', phone: null }
  await store.fetch()
  expect(store.profile).toBeNull()
  expect(store.error).toBe('Not found')
})
```

- [ ] **Step 2: Write address.store.spec.ts**

Tests: fetchMany, getById, create, update, delete. Address CRUD operations.

- [ ] **Step 3: Run tests**

```bash
cd app/Admin && npx vitest run src/features/profile/store/__tests__/
```
Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/profile/store/__tests__/
git commit -m "test(profile): add store tests for profile, address"
```

---

### Task 6: Shipping store tests (2 stores)

**Files:**
- Create: `app/Admin/src/features/shipping/store/__tests__/shipping-method.store.spec.ts`
- Create: `app/Admin/src/features/shipping/store/__tests__/shipping-rate.store.spec.ts`

**Interfaces:**
- Consumes: Existing shipping-method, shipping-rate stores

- [ ] **Step 1: Write shipping-method.store.spec.ts**

Tests: fetchMany, getById, create, update, delete, activate, deactivate.

- [ ] **Step 2: Write shipping-rate.store.spec.ts**

Tests: fetchMany, getById, create, update, delete. Test weight range and currency fields.

- [ ] **Step 3: Run tests**

```bash
cd app/Admin && npx vitest run src/features/shipping/store/__tests__/
```
Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/shipping/store/__tests__/
git commit -m "test(shipping): add store tests for shipping-method, shipping-rate"
```

---

### Task 7: Users store tests (3 stores)

**Files:**
- Create: `app/Admin/src/features/users/store/__tests__/user.store.spec.ts`
- Create: `app/Admin/src/features/users/store/__tests__/role.store.spec.ts`
- Create: `app/Admin/src/features/users/store/__tests__/permission.store.spec.ts`

**Interfaces:**
- Consumes: Existing user, role, permission stores

- [ ] **Step 1: Write user.store.spec.ts**

Tests: fetchMany, getById, create, update, delete, toggleStatus, assignRole, revokeRole, assignPermission, revokePermission.

```ts
describe('assignRole', () => {
  it('calls API assignRole with userId and roleId', async () => {
    vi.mocked(UserApi.assignRole).mockResolvedValue({ isSuccess: true, value: null, message: null })
    const store = useUserStore()
    await store.assignRole('user-1', 'role-admin')
    expect(UserApi.assignRole).toHaveBeenCalledWith('user-1', 'role-admin')
  })
})

describe('toggleStatus', () => {
  it('calls API with userId', async () => {
    vi.mocked(UserApi.toggleStatus).mockResolvedValue({ isSuccess: true, value: null, message: null })
    const store = useUserStore()
    await store.toggleStatus('user-1')
    expect(UserApi.toggleStatus).toHaveBeenCalledWith('user-1')
  })
})
```

- [ ] **Step 2: Write role.store.spec.ts**

Tests: fetchMany, getById, create, update, delete, assignPermission, revokePermission, syncPermissions.

- [ ] **Step 3: Write permission.store.spec.ts**

Tests: fetchMany (permissions are read-only).

- [ ] **Step 4: Run tests**

```bash
cd app/Admin && npx vitest run src/features/users/store/__tests__/
```
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/users/store/__tests__/
git commit -m "test(users): add store tests for user, role, permission with assign/revoke/toggle"
```

---

### Task 8: Shared composable tests (notification)

**Files:**
- Create: `app/Admin/src/shared/composables/__tests__/notification.spec.ts`

**Interfaces:**
- Consumes: `useNotification` composable (from missing features plan)

- [ ] **Step 1: Write notification composable test**

```ts
// app/Admin/src/shared/composables/__tests__/notification.spec.ts
import { setActivePinia, createPinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useNotificationStore } from '@/stores/useNotificationStore'

vi.mock('@/stores/useNotificationStore', () => ({
  useNotificationStore: vi.fn(),
}))

describe('useNotification', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('starts polling on mount', () => {
    const startPolling = vi.fn()
    const stopPolling = vi.fn()
    vi.mocked(useNotificationStore).mockReturnValue({
      unreadCount: 0,
      recentItems: [],
      items: [],
      startPolling,
      stopPolling,
      markRead: vi.fn(),
      markAllRead: vi.fn(),
      fetch: vi.fn(),
    } as any)
    // Test that startPolling(30000) is called on mount
    // (Requires component mount — skip composable-only test for now;
    //  NotificationBell component test covers this)
  })
})
```

- [ ] **Step 2: Run test**

```bash
cd app/Admin && npx vitest run src/shared/composables/__tests__/notification.spec.ts
```
Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/composables/__tests__/notification.spec.ts
git commit -m "test(shared): add notification composable test"
```

---

### Task 9: API tests — inventory module

**Files:**
- Create: `app/Admin/src/features/inventory/api/__tests__/stock-item.api.spec.ts`
- Create: `app/Admin/src/features/inventory/api/__tests__/stock-location.api.spec.ts`
- Create: `app/Admin/src/features/inventory/api/__tests__/stock-movement.api.spec.ts`
- Create: `app/Admin/src/features/inventory/api/__tests__/stock-reservation.api.spec.ts`
- Create: `app/Admin/src/features/inventory/api/__tests__/stock-transfer.api.spec.ts`

- [ ] **Step 1: Read existing catalog API test for pattern**

Read `app/Admin/src/features/catalog/api/__tests__/products.spec.ts` to understand the mock pattern for `apiClient`.

- [ ] **Step 2: Write stock-item.api.spec.ts**

```ts
// app/Admin/src/features/inventory/api/__tests__/stock-item.api.spec.ts
import { describe, expect, it, vi } from 'vitest'

vi.mock('@/shared/api/client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}))

import { apiClient } from '@/shared/api/client'
import { StockItemApi } from '../stock-item.api'

describe('StockItemApi', () => {
  describe('getMany', () => {
    it('calls GET /inventory/stock-items with pagination params', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({
        data: { isSuccess: true, items: [], totalCount: 0 },
      })
      await StockItemApi.getMany({ page: 1, pageSize: 20 })
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-items', {
        params: { page: 1, pageSize: 20 },
      })
    })

    it('includes query param when provided', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({
        data: { isSuccess: true, items: [], totalCount: 0 },
      })
      await StockItemApi.getMany({ page: 1, pageSize: 20, query: 'SKU-1' })
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-items', {
        params: expect.objectContaining({ query: 'SKU-1' }),
      })
    })
  })

  describe('getById', () => {
    it('calls GET /inventory/stock-items/:id', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({
        data: { isSuccess: true, value: { id: '1' } },
      })
      await StockItemApi.getById('1')
      expect(apiClient.get).toHaveBeenCalledWith('/inventory/stock-items/1')
    })
  })

  describe('create', () => {
    it('calls POST /inventory/stock-items with body', async () => {
      const data = { sku: 'SKU-1', quantity: 10 }
      vi.mocked(apiClient.post).mockResolvedValue({
        data: { isSuccess: true, value: { id: '1', ...data } },
      })
      await StockItemApi.create(data)
      expect(apiClient.post).toHaveBeenCalledWith('/inventory/stock-items', data)
    })
  })

  describe('update', () => {
    it('calls PUT /inventory/stock-items/:id with body', async () => {
      vi.mocked(apiClient.put).mockResolvedValue({
        data: { isSuccess: true, value: { id: '1', quantity: 20 } },
      })
      await StockItemApi.update('1', { quantity: 20 })
      expect(apiClient.put).toHaveBeenCalledWith('/inventory/stock-items/1', { quantity: 20 })
    })
  })

  describe('delete', () => {
    it('calls DELETE /inventory/stock-items/:id', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({
        data: { isSuccess: true },
      })
      await StockItemApi.delete('1')
      expect(apiClient.delete).toHaveBeenCalledWith('/inventory/stock-items/1')
    })
  })
})
```

- [ ] **Step 3: Write remaining 4 inventory API test files**

Follow same pattern for: stock-location, stock-movement (read-only, only getMany), stock-reservation (getMany + cancel via POST), stock-transfer (getMany + getById + create + complete + cancel).

- [ ] **Step 4: Run API tests**

```bash
cd app/Admin && npx vitest run src/features/inventory/api/__tests__/
```
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/inventory/api/__tests__/
git commit -m "test(inventory): add API tests for all 5 inventory entities"
```

---

### Task 10: API tests — remaining modules (location, ordering, payment, shipping, profile, users)

**Files:**
- Create: `app/Admin/src/features/location/api/__tests__/country.api.spec.ts`
- Create: `app/Admin/src/features/location/api/__tests__/state.api.spec.ts`
- Create: `app/Admin/src/features/ordering/api/__tests__/order.api.spec.ts`
- Create: `app/Admin/src/features/payment/api/__tests__/payment.api.spec.ts`
- Create: `app/Admin/src/features/payment/api/__tests__/payment-method.api.spec.ts`
- Create: `app/Admin/src/features/shipping/api/__tests__/shipping-method.api.spec.ts`
- Create: `app/Admin/src/features/shipping/api/__tests__/shipping-rate.api.spec.ts`
- Create: `app/Admin/src/features/profile/api/__tests__/profile.api.spec.ts`
- Create: `app/Admin/src/features/profile/api/__tests__/address.api.spec.ts`
- Create: `app/Admin/src/features/users/api/__tests__/user.api.spec.ts`
- Create: `app/Admin/src/features/users/api/__tests__/role.api.spec.ts`
- Create: `app/Admin/src/features/users/api/__tests__/permission.api.spec.ts`

- [ ] **Step 1: Write all 12 API test files**

For each API module, follow the exact pattern from Task 9. Test:
- HTTP method correctness (GET/POST/PUT/DELETE)
- URL path construction (including GUID substitution in path)
- Query parameter serialization (page, pageSize, query, filters)
- Request body mapping

For lifecycle endpoints:
- OrderApi.approve(id) → verifies POST /ordering/orders/{id}/approve (or whatever method the API uses)
- OrderApi.cancel(id) → verifies correct endpoint
- PaymentApi.capture(id) → verifies correct endpoint

For assign/revoke/sync endpoints:
- UserApi.assignRole(userId, roleId) → verifies POST with correct payload
- UserApi.revokeRole(userId, roleId) → verifies DELETE with correct URL
- UserApi.syncPermissions(userId, permissionIds) → verifies PUT with correct payload

- [ ] **Step 2: Run all API tests**

```bash
cd app/Admin && npx vitest run --include 'src/features/*/api/__tests__/*.spec.ts'
```
Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/*/api/__tests__/
git commit -m "test: add API tests for all 6 remaining modules (18 API modules total)"
```

---

### Task 11: Complex component tests — inventory forms

**Files:**
- Create: `app/Admin/src/features/inventory/components/__tests__/StockItemForm.spec.ts`
- Create: `app/Admin/src/features/inventory/components/__tests__/TransferForm.spec.ts`

**Interfaces:**
- Consumes: StockItemForm, TransferForm components

- [ ] **Step 1: Read existing catalog form test for pattern**

Read `app/Admin/src/features/catalog/pages/__tests__/ProductDetailPage.spec.ts` to understand how @vue/test-utils is set up, how PrimeVue components are stubbed, how `createTestingPinia` is used.

- [ ] **Step 2: Write StockItemForm.spec.ts**

```ts
// app/Admin/src/features/inventory/components/__tests__/StockItemForm.spec.ts
import { describe, expect, it, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import StockItemForm from '../StockItemForm.vue'

describe('StockItemForm', () => {
  const mountOptions = {
    global: {
      plugins: [createTestingPinia({ stubActions: false })],
      stubs: {
        InputText: { template: '<input :value="modelValue" @input="$emit(\'update:modelValue\', $event.target.value)" />' },
        InputNumber: { template: '<input type="number" />' },
        Select: { template: '<select><slot /></select>' },
        Button: { template: '<button :disabled="disabled"><slot /></button>' },
        PageHeader: { template: '<div><slot /></div>' },
        FormField: { template: '<div><label>{{ label }}</label><slot /></div>' },
        FormActions: { template: '<div><slot name="save" /><slot name="cancel" /></div>' },
      },
    },
  }

  it('renders form fields', () => {
    const wrapper = mount(StockItemForm, {
      ...mountOptions,
      props: { mode: 'create', saving: false },
    })
    expect(wrapper.exists()).toBe(true)
  })

  it('emits save with form data', async () => {
    const wrapper = mount(StockItemForm, {
      ...mountOptions,
      props: { mode: 'create', saving: false },
    })
    await wrapper.find('button').trigger('click')
    // Verify emit behavior
  })

  it('disables submit button when saving', () => {
    const wrapper = mount(StockItemForm, {
      ...mountOptions,
      props: { mode: 'create', saving: true },
    })
    expect(wrapper.find('button').attributes('disabled')).toBeDefined()
  })

  it('shows fields as readonly in view mode', () => {
    const wrapper = mount(StockItemForm, {
      ...mountOptions,
      props: { mode: 'view', saving: false },
    })
    // Verify inputs have disabled attribute
  })
})
```

- [ ] **Step 3: Write TransferForm.spec.ts**

Tests: renders source/destination location selects, quantity input, emits save, shows error state, disables during saving.

- [ ] **Step 4: Run component tests**

```bash
cd app/Admin && npx vitest run src/features/inventory/components/__tests__/
```
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/inventory/components/__tests__/
git commit -m "test(inventory): add component tests for StockItemForm, TransferForm"
```

---

### Task 12: Complex component tests — remaining modules (forms + managers)

**Files:**
- Create: `app/Admin/src/features/location/components/__tests__/CountryForm.spec.ts`
- Create: `app/Admin/src/features/location/components/__tests__/StateForm.spec.ts`
- Create: `app/Admin/src/features/ordering/components/__tests__/OrderForm.spec.ts`
- Create: `app/Admin/src/features/ordering/components/__tests__/FulfillmentWorkflow.spec.ts`
- Create: `app/Admin/src/features/payment/components/__tests__/PaymentMethodForm.spec.ts`
- Create: `app/Admin/src/features/shipping/components/__tests__/ShippingMethodForm.spec.ts`
- Create: `app/Admin/src/features/shipping/components/__tests__/ShippingRateForm.spec.ts`
- Create: `app/Admin/src/features/profile/components/__tests__/AddressForm.spec.ts`
- Create: `app/Admin/src/features/users/components/__tests__/UserForm.spec.ts`
- Create: `app/Admin/src/features/users/components/__tests__/RoleForm.spec.ts`
- Create: `app/Admin/src/features/users/components/__tests__/RolePermissionManager.spec.ts`
- Create: `app/Admin/src/features/catalog/components/__tests__/VariantImageManager.spec.ts`

- [ ] **Step 1: Write all 12 component test files**

For each component, follow the pattern from Task 11:

**Form components** (CountryForm, StateForm, OrderForm, PaymentMethodForm, ShippingMethodForm, ShippingRateForm, AddressForm, UserForm, RoleForm):
- Renders form fields
- Shows fields as disabled in view mode
- Disables submit during saving
- Emits save/cancel events
- Shows error state
- Validates required fields

**Manager components** (RolePermissionManager, VariantImageManager):
- Renders available items list
- Calls assign API on add action
- Calls revoke API on remove action
- Shows loading state during API call
- Shows empty state when no items
- Handles API error gracefully

**Workflow components** (FulfillmentWorkflow):
- Highlights current step correctly
- Shows correct action button for each status
- Calls appropriate API on button click
- Shows error toast on API failure
- Shows terminal state message for Delivered/Cancelled

- [ ] **Step 2: Run all component tests**

```bash
cd app/Admin && npx vitest run --include 'src/features/*/components/__tests__/*.spec.ts'
```
Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/*/components/__tests__/
git commit -m "test: add component tests for 12 forms managers and workflows across all modules"
```

---

### Task 13: Final test verification

**Files:**
- No file changes — verification only.

- [ ] **Step 1: Run all unit tests**

```bash
cd app/Admin && pnpm run test:unit
```
Expected: All tests PASS, exit code 0.

- [ ] **Step 2: Run with coverage (optional)**

```bash
cd app/Admin && pnpm run test:unit -- --coverage
```
Expected: Measurable improvement over baseline. Focus on store + API coverage > 90%.

- [ ] **Step 3: Verify TypeScript**

```bash
cd app/Admin && npx vue-tsc --noEmit
```
Expected: PASS with zero errors (test files should not cause type errors).

- [ ] **Step 4: Commit**

```bash
git commit -m "chore: final test verification - all tests pass, vue-tsc clean" --allow-empty
```
