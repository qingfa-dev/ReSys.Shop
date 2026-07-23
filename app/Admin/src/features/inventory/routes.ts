import type { RouteRecordRaw } from 'vue-router'

export const inventoryRoutes: RouteRecordRaw = {
  path: 'inventory',
  children: [
    { path: '', redirect: { name: 'inventory.dashboard' } },
    {
      path: 'dashboard',
      name: 'inventory.dashboard',
      component: () => import('@/features/inventory/pages/DashboardPage.vue'),
    },
    {
      path: 'stocks',
      name: 'inventory.stocks.list',
      component: () => import('@/features/inventory/pages/StockListPage.vue'),
    },
    {
      path: 'stocks/new',
      name: 'inventory.stocks.create',
      component: () => import('@/features/inventory/pages/StockItemDetailPage.vue'),
    },
    {
      path: 'stocks/:id',
      name: 'inventory.stocks.view',
      component: () => import('@/features/inventory/pages/StockItemDetailPage.vue'),
    },
    {
      path: 'stocks/:id/edit',
      name: 'inventory.stocks.edit',
      component: () => import('@/features/inventory/pages/StockItemDetailPage.vue'),
    },
    {
      path: 'locations',
      name: 'inventory.locations.list',
      component: () => import('@/features/inventory/pages/LocationListPage.vue'),
    },
    {
      path: 'locations/new',
      name: 'inventory.locations.create',
      component: () => import('@/features/inventory/pages/LocationDetailPage.vue'),
    },
    {
      path: 'locations/:id',
      name: 'inventory.locations.view',
      component: () => import('@/features/inventory/pages/LocationDetailPage.vue'),
    },
    {
      path: 'locations/:id/edit',
      name: 'inventory.locations.edit',
      component: () => import('@/features/inventory/pages/LocationDetailPage.vue'),
    },
    {
      path: 'movements',
      name: 'inventory.movements.list',
      component: () => import('@/features/inventory/pages/MovementListPage.vue'),
    },
    {
      path: 'transfers',
      name: 'inventory.transfers.list',
      component: () => import('@/features/inventory/pages/TransferListPage.vue'),
    },
    {
      path: 'transfers/new',
      name: 'inventory.transfers.create',
      component: () => import('@/features/inventory/pages/TransferDetailPage.vue'),
    },
    {
      path: 'transfers/:id',
      name: 'inventory.transfers.view',
      component: () => import('@/features/inventory/pages/TransferDetailPage.vue'),
    },
    {
      path: 'transfers/:id/edit',
      name: 'inventory.transfers.edit',
      component: () => import('@/features/inventory/pages/TransferDetailPage.vue'),
    },
    {
      path: 'reservations',
      name: 'inventory.reservations.list',
      component: () => import('@/features/inventory/pages/StockReservationListPage.vue'),
    },
  ],
}
