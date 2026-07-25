---
goal: Implement admin Ordering module frontend API services
version: 1.0
date_created: 2026-07-25
status: Planned
tags: feature, api, ordering, admin
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Implement the admin Ordering module in `app/Admin/src/features/ordering/`. 19 backend endpoints: Orders CRUD + lifecycle actions (cancel/complete/approve/resume), Line Items CRUD, address/shipping-method updates, status updates, Fulfillment Queue, and Ordering Dashboard. All pages are placeholder shells.

Backend route prefix: `api/ordering`

## 1. Requirements & Constraints

- **REQ-001**: Every backend endpoint must have a frontend API method
- **REQ-002**: All API methods use shared `apiClient`
- **REQ-003**: Response types as camelCase interfaces matching backend C# records
- **REQ-004**: Zod validation for entities with create/update forms
- **REQ-005**: Form-to-request mapper classes with static toCreate/toUpdate
- **REQ-006**: List pages get Pinia stores; dashboard gets a standalone API call, no store
- **REQ-007**: Replace PlaceholderPage with real component pages
- **CON-001**: Follow catalog module patterns exactly
- **CON-002**: Zero TypeScript errors
- **CON-003**: Store IDs: `'ordering-order'`
- **PAT-001** to **PAT-009**: Same as catalog patterns

## 2. Implementation Steps

### Phase 1: Orders CRUD + types + schemas + store + composable + pages

- GOAL-001: Implement Orders core: types, schemas, mappers, API, store, composable, pages, components

Backend endpoints:
- GET `/orders` — GetPaged
- GET `/orders/{id:guid}` — GetById
- POST `/orders` — Create
- PUT `/orders/{id:guid}` — UpdateDetails
- DELETE `/orders/{id:guid}` — Delete
- POST `/orders/{id:guid}/cancel` — Cancel
- POST `/orders/{id:guid}/complete` — Complete
- POST `/orders/{id:guid}/approve` — Approve
- POST `/orders/{id:guid}/resume` — Resume
- PUT `/orders/{id:guid}/status` — UpdateStatus
- PUT `/orders/{id:guid}/ship-address` — UpdateShipAddress
- PUT `/orders/{id:guid}/bill-address` — UpdateBillAddress
- PUT `/orders/{id:guid}/shipping-method` — UpdateShippingMethod

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `types/order.response.ts` — `OrderResponse`: id, orderNumber, status, customerId, customerName?, customerEmail?, subtotal, total, taxTotal, shippingTotal, currency, paymentMethod?, shippingMethod?, shipAddress: AddressResponse?, billAddress: AddressResponse?, lineItems: OrderLineItemResponse[], notes?, createdAt, updatedAt; `OrderLineItemResponse`: id, variantId, variantSku?, variantName?, quantity, unitPrice, totalPrice; `AddressResponse`: firstName, lastName, address1, address2?, city, state?, postalCode, country, phone? | | |
| TASK-002 | Create `types/order.request.ts` — `CreateOrderRequest`: customerId, shippingMethodId?, lineItems: { variantId, quantity }[], notes?; `UpdateOrderStatusRequest`: status: string; `UpdateAddressRequest`: full AddressResponse fields | | |
| TASK-003 | Create `schemas/order.fields.ts` — fields: customerId (required), lineItems array, notes (optional), status (optional for update) | | |
| TASK-004 | Create `schemas/order.forms.ts` — `OrderForms` with create()/update() schemas | | |
| TASK-005 | Create `mappers/order.mapper.ts` — `OrderFormMapper` with toCreate/toUpdate | | |
| TASK-006 | Create `api/order.api.ts` — `OrderApi`: getMany(query), get(id), create(data), update(id, data), delete(id), cancel(id), complete(id), approve(id), resume(id), updateStatus(id, data), updateShipAddress(id, data), updateBillAddress(id, data), updateShippingMethod(id, data) | | |
| TASK-007 | Create `store/order.store.ts` — `useOrderStore` with items/loading/error/totalRecords/query/fetchMany/mutations | | |
| TASK-008 | Create `composables/useOrder.ts` — returns { id, mode, route, router, toast, api: OrderApi } | | |
| TASK-009 | Create `components/OrderForm.vue` — order header fields (customer, status, notes, shipping method) + inline line items section + address read/edit sections + lifecycle action buttons (approve, complete, cancel, resume based on status) | | |
| TASK-010 | Create `components/OrderListTable.vue` — DataTable: orderNumber, customerName, status (StatusTag), total, createdAt, ActionMenu | | |
| TASK-011 | Replace `pages/OrderListPage.vue` — PageHeader + TableToolbar + OrderListTable | | |
| TASK-012 | Replace `pages/OrderDetailPage.vue` — OrderForm | | |
| TASK-013 | Update routes, barrel exports | | |
| TASK-014 | Verify: type-check passes | | |

### Phase 2: Order Line Items + Fulfillment Queue

- GOAL-002: Implement Order Line Items (nested under orders) + Fulfillment Queue

Backend endpoints:
- GET `/orders/{id:guid}/line-items` — GetLineItems
- GET `/orders/{id:guid}/line-items/{lineItemId:guid}` — GetLineItemById
- POST `/orders/{id:guid}/line-items` — AddLineItem
- PUT `/orders/{id:guid}/line-items/{lineItemId:guid}` — UpdateLineItem
- DELETE `/orders/{id:guid}/line-items/{lineItemId:guid}` — RemoveLineItem

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Create `types/order-line-item.request.ts` — `AddLineItemRequest`: variantId, quantity; `UpdateLineItemRequest`: quantity | | |
| TASK-016 | Create `api/order-line-item.api.ts` — `OrderLineItemApi`: getMany(orderId), get(orderId, lineItemId), create(orderId, data), update(orderId, lineItemId, data), delete(orderId, lineItemId) | | |
| TASK-017 | Create `components/OrderLineItemManager.vue` — inline inside OrderForm; DataTable of line items, add/edit/delete with product variant selector | | |
| TASK-018 | Create `pages/FulfillmentQueuePage.vue` — replaces placeholder; DataTable of orders ready for fulfillment; columns: orderNumber, customerName, items count, status, actions (mark shipped) | | |
| TASK-019 | Update barrels, verify type-check passes | | |

### Phase 3: Ordering Dashboard

- GOAL-003: Implement Ordering Dashboard API integration

Backend endpoint: GET `/dashboard` returns `OrderingDashboardResponse`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | Create `types/ordering-dashboard.response.ts` — `OrderingDashboardResponse`: totalOrders, pendingOrders, completedOrders, cancelledOrders, totalRevenue, todayRevenue, recentOrders: RecentOrderData[]; `RecentOrderData`: id, orderNumber, customerName, total, status, createdAt | | |
| TASK-021 | Create `api/ordering-dashboard.api.ts` — `OrderingDashboardApi` with get(): GET `/ordering/dashboard` | | |
| TASK-022 | Replace `pages/DashboardPage.vue` — load from API, stat cards + recent orders table | | |
| TASK-023 | Update barrels, verify type-check passes | | |

## 3. Alternatives

- **ALT-001**: Separate fulfillment as its own module — rejected: fulfillment is part of ordering lifecycle
- **ALT-002**: Use single monolithic OrderApi — chosen: all order endpoints are closely related

## 4. Dependencies

- **DEP-001**: Shared apiClient, Result/PagedResult/ListQuery from `@/shared/`

## 5. Files

- **FILE-001** to **FILE-023**: One per task

## 6. Testing

- **TEST-001**: `api/__tests__/orders.spec.ts` — verify all 14 methods
- **TEST-002**: `api/__tests__/order-line-items.spec.ts` — verify all 5 methods
- **TEST-003**: `api/__tests__/ordering-dashboard.spec.ts` — verify get()

## 7. Risks & Assumptions

- **ASSUMPTION-001**: Backend order statuses follow a known enum (Cart, Pending, Approved, Completed, Cancelled, etc.)
- **RISK-001**: Order lifecycle is complex (approve before complete, cancel only when pending) — UI must disable inappropriate action buttons based on current status

## 8. Related Specifications / Further Reading

Backend: `service/Api/src/Module/Ordering/Features/Admin/`
Route constants: `service/Api/src/Module/Ordering/Features/Shared/OrderingFeature.Admin.cs`
