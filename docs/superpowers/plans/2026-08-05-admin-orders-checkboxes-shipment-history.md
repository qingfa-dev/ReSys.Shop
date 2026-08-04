# Admin Orders: Checkboxes, Shipment State, Order History

## Scope
1. Add checkboxes to 10 list views missing selection (following CountriesList pattern)
2. Display + update shipment state in ordering
3. New OrderHistory entity + API + UI tab

## Task 1: Add checkboxes to all list views missing selection

Pattern from CountriesList.vue:
- Add `selectedItems` ref
- Add `<Column selection-mode="multiple" header-style="width: 3rem" />` as first column
- Add `v-model:selection="selectedItems"` to DataTable
- For lists with delete: add bulk delete using `selectedItems`

Files (10):
1. `ordering/views/OrdersList.vue` — read-only, no delete
2. `payment/views/PaymentMethodsList.vue` — has delete
3. `payment/views/PaymentsList.vue` — read-only
4. `profile/views/AddressesList.vue` — has delete
5. `profile/views/ProfilesList.vue` — read-only
6. `shipping/views/ShippingMethodsList.vue` — has delete
7. `shipping/views/ShippingRatesList.vue` — has delete
8. `identity/views/PermissionsList.vue` — read-only
9. `inventory/views/StockMovementsList.vue` — read-only
10. `inventory/views/StockReservationsList.vue` — read-only
11. `inventory/views/StockTransfersList.vue` — edit-only

## Task 2: Backend — Shipment state update endpoint

- New feature: `Features/Admin/Orders/UpdateShipmentState/`
- Request: `{ shipmentState: string }` (values: pending, ready, partial, shipped, delivered, canceled)
- Handler: update `order.ShipmentState`, persist
- Register route in `OrderingFeature.Admin.cs`

## Task 3: Frontend — Display shipment state

- OrdersList.vue: add ShipmentState column with Tag
- OrderDetail.vue Overview tab: show ShipmentState field
- OrderDetail.vue header: add ShipmentState dropdown to update it

## Task 4: Backend — OrderHistory entity

- New entity: `Domain/Orders/OrderHistory.cs`
  - Properties: Id, OrderId, Action, Description, CreatedBy, CreatedAtUtc
- New schema: `TableNames.OrderHistory = "order_history"`
- EF configuration + migration
- New feature: `Features/Admin/Orders/GetHistory/`
  - GET `api/ordering/orders/{id}/history` → `PagedResult<OrderHistory>`
- Record events on status changes (in existing handlers)

## Task 5: Frontend — Order History tab

- New composable: `useOrderHistory.ts`
- New API method: `OrderApi.getOrderHistory(id, query)`
- OrderDetail.vue: add Tab 3 "History" with DataTable
- Types: `OrderHistoryItem` interface
