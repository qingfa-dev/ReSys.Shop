import type { RouteRecordRaw } from 'vue-router'

export const inventoryRoutes: RouteRecordRaw = {
  path: 'inventory',
  meta: { breadcrumb: 'Inventory' },
  children: [
    {
      path: '',
      name: 'inventory.dashboard',
      component: () => import('./dashboard/pages/InventoryDashboardPage.vue'),
      meta: { breadcrumb: 'Overview' },
    },
    {
      path: 'stocks',
      name: 'inventory.stocks.list',
      component: () => import('./stock-items/pages/StockItemListPage.vue'),
      meta: { breadcrumb: 'Stock Levels' }
    },
    {
      path: 'stocks/import',
      name: 'inventory.stocks.import',
      component: () => import('./stock-items/pages/StockImportPage.vue'),
      meta: { breadcrumb: 'Import Stock' },
    },
    {
      path: 'units',
      name: 'inventory.units.list',
      component: () => import('./inventory-units/pages/InventoryUnitListPage.vue'),
      meta: { breadcrumb: 'Serialized Units' }
    },
    {
      path: 'movements',
      name: 'inventory.movements.list',
      component: () => import('./stock-movements/pages/StockMovementListPage.vue'),
      meta: { breadcrumb: 'Stock Movements' },
    },
    {
      path: 'movements/:id',
      name: 'inventory.movements.detail',
      component: () => import('./stock-movements/pages/StockMovementDetailPage.vue'),
      meta: { breadcrumb: 'Movement Detail' },
    },
    {
      path: 'locations',
      meta: { breadcrumb: 'Warehouses' },
      component: () => import('./stock-locations/pages/StockLocationManagerPage.vue'),
      children: [
        {
          path: '',
          name: 'inventory.locations.list',
          component: () => import('./stock-locations/pages/StockLocationListPage.vue'),
          meta: { breadcrumb: 'All Locations' },
        },
        {
          path: 'create',
          name: 'inventory.locations.create',
          component: () => import('./stock-locations/pages/StockLocationFormPage.vue'),
          props: { hideHeader: true },
          meta: { breadcrumb: 'Add Location' }
        },
        {
          path: ':id/edit',
          name: 'inventory.locations.edit',
          component: () => import('./stock-locations/pages/StockLocationFormPage.vue'),
          props: { hideHeader: true },
          meta: { breadcrumb: 'Edit Location' }
        },
      ]
    },
    {
      path: 'transfers',
      name: 'inventory.transfers.list',
      component: () => import('./stock-transfers/pages/StockTransferListPage.vue'),
      meta: { breadcrumb: 'Logistics' }
    },
    {
      path: 'transfers/create',
      name: 'inventory.transfers.create',
      component: () => import('./stock-transfers/pages/StockTransferFormPage.vue'),
      meta: { breadcrumb: 'Initiate Transfer' }
    },
    {
      path: 'transfers/:id',
      name: 'inventory.transfers.detail',
      component: () => import('./stock-transfers/pages/StockTransferDetailPage.vue'),
      meta: { breadcrumb: 'Transfer Details' }
    }
  ]
}
