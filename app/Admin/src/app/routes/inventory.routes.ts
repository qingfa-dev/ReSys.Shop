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
      path: 'stocks/import',
      name: 'inventory.stocks.import',
      component: () => import('@/features/inventory/pages/StockImportPage.vue'),
    },
    {
      path: 'locations',
      name: 'inventory.locations.list',
      component: () => import('@/features/inventory/pages/LocationListPage.vue'),
    },
    {
      path: 'units',
      name: 'inventory.units.list',
      component: () => import('@/features/inventory/pages/UnitListPage.vue'),
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
  ],
}
