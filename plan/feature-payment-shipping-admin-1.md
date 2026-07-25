---
goal: Implement admin Payment and Shipping module frontend API services
version: 1.0
date_created: 2026-07-25
status: Planned
tags: feature, api, payment, shipping, admin
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Implement admin Payment and Shipping modules in parallel. 24 total backend endpoints across 4 entities: Payments (list + capture/void/refund), PaymentMethods (CRUD + activate/deactivate), ShippingMethods (CRUD + activate/deactivate), ShippingRates (CRUD). All pages are placeholder shells.

Backend route prefixes: `api/payment`, `api/shipping`

## 1. Requirements & Constraints

- **REQ-001**: Every backend endpoint must have a frontend API method
- **REQ-002**: All API methods use shared `apiClient` from `@/shared/api/client`
- **REQ-003**: Response types as camelCase interfaces matching backend C# records
- **REQ-004**: Zod validation for entities with create/update forms
- **REQ-005**: Form-to-request mapper classes with static toCreate/toUpdate
- **REQ-006**: List pages get Pinia stores; payment action endpoints (capture/void/refund) are lifecycle actions on detail page
- **REQ-007**: Replace PlaceholderPage with real components
- **CON-001**: Follow catalog module patterns exactly
- **CON-002**: Store IDs: `'payment-payment'`, `'payment-method'`, `'shipping-method'`, `'shipping-rate'`
- **CON-003**: Follow activate/deactivate PATCH pattern from ProductApi.activate
- **CON-004**: Zero TypeScript errors
- **PAT-001** to **PAT-009**: Same as catalog patterns

## 2. Implementation Steps

### Phase 1: Payments (read-only list + lifecycle actions)

- GOAL-001: Implement Payments list + detail with capture/void/refund actions

Backend endpoints:
- GET `/payments` — GetPaged
- GET `/payments/{id:guid}` — GetById
- POST `/payments/{id:guid}/capture` — Capture
- POST `/payments/{id:guid}/void` — Void
- POST `/payments/{id:guid}/refund` — Refund

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `types/payment.response.ts` — `PaymentResponse`: id, orderId?, orderNumber?, paymentMethodId, paymentMethodName?, amount, currency, status, authorizationCode?, capturedAt?, voidedAt?, refundedAt?, notes?, createdAt, updatedAt | | |
| TASK-002 | Create `types/payment.request.ts` — `CapturePaymentRequest`/`VoidPaymentRequest`/`RefundPaymentRequest` (may be empty or have amount/notes) | | |
| TASK-003 | Create `api/payment.api.ts` — `PaymentApi`: getMany(query), get(id), capture(id, data?), void(id, data?), refund(id, data?) | | |
| TASK-004 | Create `store/payment.store.ts` — `usePaymentStore` (standard list state) | | |
| TASK-005 | Create `composables/usePayment.ts` — returns { id, mode, route, router, toast, api: PaymentApi } | | |
| TASK-006 | Create `components/PaymentListTable.vue` — columns: orderNumber, paymentMethodName, amount, currency, status (StatusTag), createdAt, ActionMenu | | |
| TASK-007 | Create `components/PaymentDetailForm.vue` — read-only detail + action buttons (Capture/Void/Refund) shown based on status | | |
| TASK-008 | Replace `pages/PaymentListPage.vue` — PageHeader + PaymentListTable | | |
| TASK-009 | Replace `pages/PaymentDetailPage.vue` — PaymentDetailForm | | |
| TASK-010 | Update routes, barrels | | |
| TASK-011 | Verify: type-check passes | | |

### Phase 2: Payment Methods (CRUD + activate/deactivate)

- GOAL-002: Implement Payment Methods full CRUD + status toggles

Backend endpoints:
- GET `/payment-methods` — GetPaged
- GET `/payment-methods/{id:guid}` — GetById
- POST `/payment-methods` — Create
- PUT `/payment-methods/{id:guid}` — Update
- DELETE `/payment-methods/{id:guid}` — Delete
- PATCH `/payment-methods/{id:guid}/activate` — Activate
- PATCH `/payment-methods/{id:guid}/deactivate` — Deactivate

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | Create `types/payment-method.response.ts` — `PaymentMethodResponse`: id, name, code, description?, isActive, isTestMode?, displayOrder, supportedCurrencies?, createdAt, updatedAt | | |
| TASK-013 | Create `types/payment-method.request.ts` — `CreatePaymentMethodRequest`/`UpdatePaymentMethodRequest` (alias from form) | | |
| TASK-014 | Create `schemas/payment-method.fields.ts` — fields: name (required), code (required), description (optional), isActive (boolean), isTestMode (boolean), displayOrder (number), supportedCurrencies (string) | | |
| TASK-015 | Create `schemas/payment-method.forms.ts` — `PaymentMethodForms` with create()/update() | | |
| TASK-016 | Create `mappers/payment-method.mapper.ts` — `PaymentMethodFormMapper` | | |
| TASK-017 | Create `api/payment-method.api.ts` — `PaymentMethodApi`: getMany(query), get(id), create(data), update(id, data), delete(id), activate(id), deactivate(id) — PATCH `/payment-methods/${id}/activate` and `/deactivate` | | |
| TASK-018 | Create `store/payment-method.store.ts` — `usePaymentMethodStore` | | |
| TASK-019 | Create `composables/usePaymentMethod.ts` | | |
| TASK-020 | Create `components/PaymentMethodForm.vue` — fields: name, code, description, isActive, isTestMode, displayOrder, supportedCurrencies | | |
| TASK-021 | Create `components/PaymentMethodListTable.vue` — columns: name, code, isActive (icon), isTestMode, displayOrder, ActionMenu | | |
| TASK-022 | Replace `pages/PaymentMethodListPage.vue` and `PaymentMethodDetailPage.vue` | | |
| TASK-023 | Update routes, barrels | | |
| TASK-024 | Verify: type-check passes | | |

### Phase 3: Shipping Methods (CRUD + activate/deactivate)

- GOAL-003: Implement Shipping Methods full CRUD + status toggles

Backend endpoints:
- GET `/shipping-methods` — GetPaged
- GET `/shipping-methods/{id:guid}` — GetById
- POST `/shipping-methods` — Create
- PUT `/shipping-methods/{id:guid}` — Update
- DELETE `/shipping-methods/{id:guid}` — Delete
- PATCH `/shipping-methods/{id:guid}/activate` — Activate
- PATCH `/shipping-methods/{id:guid}/deactivate` — Deactivate

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-025 | Create `types/shipping-method.response.ts` — `ShippingMethodResponse`: id, name, code, description?, isActive, displayOrder, estimatedDeliveryMin?, estimatedDeliveryMax?, createdAt, updatedAt | | |
| TASK-026 | Create `types/shipping-method.request.ts` — `CreateShippingMethodRequest`/`UpdateShippingMethodRequest` (alias from form) | | |
| TASK-027 | Create `schemas/shipping-method.fields.ts` — fields: name (required), code (required), description (optional), isActive (boolean), displayOrder (number), estimatedDeliveryMin (optional number), estimatedDeliveryMax (optional number) | | |
| TASK-028 | Create `schemas/shipping-method.forms.ts` — `ShippingMethodForms` with create()/update() | | |
| TASK-029 | Create `mappers/shipping-method.mapper.ts` — `ShippingMethodFormMapper` | | |
| TASK-030 | Create `api/shipping-method.api.ts` — `ShippingMethodApi`: getMany(query), get(id), create(data), update(id, data), delete(id), activate(id) PATCH `/shipping-methods/${id}/activate`, deactivate(id) PATCH `/shipping-methods/${id}/deactivate` | | |
| TASK-031 | Create `store/shipping-method.store.ts` — `useShippingMethodStore` | | |
| TASK-032 | Create `composables/useShippingMethod.ts` | | |
| TASK-033 | Create `components/ShippingMethodForm.vue` — fields: name, code, description, isActive, displayOrder, estimatedDeliveryMin, estimatedDeliveryMax | | |
| TASK-034 | Create `components/ShippingMethodListTable.vue` — columns: name, code, isActive (icon), displayOrder, estimatedDelivery range, ActionMenu | | |
| TASK-035 | Replace ShippingMethodListPage.vue and ShippingMethodDetailPage.vue | | |
| TASK-036 | Update routes, barrels | | |
| TASK-037 | Verify: type-check passes | | |

### Phase 4: Shipping Rates (CRUD)

- GOAL-004: Implement Shipping Rates CRUD

Backend endpoints:
- GET `/shipping-rates` — GetPaged
- GET `/shipping-rates/{id:guid}` — GetById
- POST `/shipping-rates` — Create
- PUT `/shipping-rates/{id:guid}` — Update
- DELETE `/shipping-rates/{id:guid}` — Delete

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-038 | Create `types/shipping-rate.response.ts` — `ShippingRateResponse`: id, shippingMethodId, shippingMethodName?, name, rate, currency, minOrderAmount?, maxOrderAmount?, minWeight?, maxWeight?, createdAt, updatedAt | | |
| TASK-039 | Create `types/shipping-rate.request.ts` — alias from form schema | | |
| TASK-040 | Create `schemas/shipping-rate.fields.ts` — fields: shippingMethodId (required), name (required), rate (required number), currency (required), minOrderAmount (optional), maxOrderAmount (optional), minWeight (optional), maxWeight (optional) | | |
| TASK-041 | Create `schemas/shipping-rate.forms.ts` — `ShippingRateForms` with create()/update() | | |
| TASK-042 | Create `mappers/shipping-rate.mapper.ts` | | |
| TASK-043 | Create `api/shipping-rate.api.ts` — `ShippingRateApi`: getMany(query), get(id), create(data), update(id, data), delete(id) | | |
| TASK-044 | Create `store/shipping-rate.store.ts` — `useShippingRateStore` | | |
| TASK-045 | Create `composables/useShippingRate.ts` | | |
| TASK-046 | Create `components/ShippingRateForm.vue` — fields: shippingMethodId (select), name, rate, currency, minOrderAmount, maxOrderAmount, minWeight, maxWeight | | |
| TASK-047 | Create `components/ShippingRateListTable.vue` — columns: name, shippingMethodName, rate, currency, minOrderAmount-maxOrderAmount, ActionMenu | | |
| TASK-048 | Replace ShippingRateListPage.vue and ShippingRateDetailPage.vue | | |
| TASK-049 | Update routes, barrels | | |
| TASK-050 | Verify: type-check passes | | |

## 3. Alternatives

- **ALT-001**: Combine Payment and Shipping into single plan — chosen: they share the activate/deactivate pattern and are similar in size

## 4. Dependencies

- **DEP-001**: Shared apiClient, Result/PagedResult/ListQuery from `@/shared/`

## 5. Files

- **FILE-001** to **FILE-050**: One per task

## 6. Testing

- **TEST-001**: `api/__tests__/payments.spec.ts` — verify all 5 methods
- **TEST-002**: `api/__tests__/payment-methods.spec.ts` — verify all 7 methods
- **TEST-003**: `api/__tests__/shipping-methods.spec.ts` — verify all 7 methods
- **TEST-004**: `api/__tests__/shipping-rates.spec.ts` — verify all 5 methods

## 7. Risks & Assumptions

- **ASSUMPTION-001**: Backend Payment capture/void/refund follow the same `Result<T>` pattern
- **ASSUMPTION-002**: Shipping rates may be associated with a shipping method via dropdown; the options come from ShippingMethodApi.getMany

## 8. Related Specifications / Further Reading

Backend Payment: `service/Api/src/Module/Payment/Features/Admin/`
Backend Shipping: `service/Api/src/Module/Shipping/Features/Admin/`
Route constants: `service/Api/src/Module/Payment/Features/Shared/PaymentFeature.Admin.cs`
Route constants: `service/Api/src/Module/Shipping/Features/Shared/ShippingFeature.Admin.cs`
