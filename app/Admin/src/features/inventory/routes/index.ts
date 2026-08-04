import type { RouteRecordRaw } from 'vue-router'

const StockItemsList = () => import('../views/StockItemsList.vue')
const StockItemDetail = () => import('../views/StockItemDetail.vue')
const StockLocationsList = () => import('../views/StockLocationsList.vue')
const StockLocationDetail = () => import('../views/StockLocationDetail.vue')
const StockReservationsList = () => import('../views/StockReservationsList.vue')
const StockTransfersList = () => import('../views/StockTransfersList.vue')
const StockTransferDetail = () => import('../views/StockTransferDetail.vue')
const StockMovementsList = () => import('../views/StockMovementsList.vue')

export const inventoryRoutes: RouteRecordRaw[] = [
  {
    path: 'inventory',
    redirect: { name: 'inventory-stock-items' },
  },
  {
    path: 'inventory/stock-items',
    name: 'inventory-stock-items',
    component: StockItemsList,
    meta: { title: 'Stock Items' },
  },
  {
    path: 'inventory/stock-items/:id',
    name: 'inventory-stock-item-detail',
    component: StockItemDetail,
    meta: { title: 'Stock Item Detail' },
  },
  {
    path: 'inventory/stock-locations',
    name: 'inventory-stock-locations',
    component: StockLocationsList,
    meta: { title: 'Stock Locations' },
  },
  {
    path: 'inventory/stock-locations/:id',
    name: 'inventory-stock-location-detail',
    component: StockLocationDetail,
    meta: { title: 'Stock Location Detail' },
  },
  {
    path: 'inventory/stock-reservations',
    name: 'inventory-stock-reservations',
    component: StockReservationsList,
    meta: { title: 'Stock Reservations' },
  },
  {
    path: 'inventory/stock-transfers',
    name: 'inventory-stock-transfers',
    component: StockTransfersList,
    meta: { title: 'Stock Transfers' },
  },
  {
    path: 'inventory/stock-transfers/:id',
    name: 'inventory-stock-transfer-detail',
    component: StockTransferDetail,
    meta: { title: 'Stock Transfer Detail' },
  },
  {
    path: 'inventory/stock-movements',
    name: 'inventory-stock-movements',
    component: StockMovementsList,
    meta: { title: 'Stock Movements' },
  },
]

export const inventoryMenuItems = [
  {
    label: 'Inventory',
    icon: 'pi pi-fw pi-warehouse',
    items: [
      { label: 'Stock Items', icon: 'pi pi-fw pi-box', to: '/inventory/stock-items' },
      { label: 'Locations', icon: 'pi pi-fw pi-map-marker', to: '/inventory/stock-locations' },
      { label: 'Reservations', icon: 'pi pi-fw pi-calendar', to: '/inventory/stock-reservations' },
      { label: 'Transfers', icon: 'pi pi-fw pi-arrows-h', to: '/inventory/stock-transfers' },
      { label: 'Movements', icon: 'pi pi-fw pi-history', to: '/inventory/stock-movements' },
    ],
  },
]
