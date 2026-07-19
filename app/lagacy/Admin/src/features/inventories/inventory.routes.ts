import type { RouteRecordRaw } from 'vue-router'

export const inventoryRoutes: RouteRecordRaw = {
  path: 'inventory',
  meta: { breadcrumb: 'Inventory' },
  children: [
    {
      path: '',
      name: 'inventory.dashboard',
      component: () => import('./dashboard/views/InventoryDashboard.View.vue'),
      meta: { breadcrumb: 'Overview' },
    },
    {
      path: 'stocks',
      name: 'inventory.stocks.list',
      component: () => import('./stock-items/views/StockItemList.View.vue'),
      meta: { breadcrumb: 'Stock Levels' }
    },
    {
      path: 'stocks/import',
      name: 'inventory.stocks.import',
      component: () => import('./stock-items/views/StockImport.View.vue'),
      meta: { breadcrumb: 'Import Stock' },
    },
    {
      path: 'units',
      name: 'inventory.units.list',
      component: () => import('./inventory-units/views/InventoryUnitList.View.vue'),
      meta: { breadcrumb: 'Serialized Units' }
    },
    {
      path: 'movements',
      name: 'inventory.movements.list',
      component: () => import('./stock-movements/views/StockMovementList.View.vue'),
      meta: { breadcrumb: 'Stock Movements' },
    },
    {
      path: 'movements/:id',
      name: 'inventory.movements.detail',
      component: () => import('./stock-movements/views/StockMovementDetail.View.vue'),
      meta: { breadcrumb: 'Movement Detail' },
    },
    {
      path: 'locations',
      meta: { breadcrumb: 'Warehouses' },
      component: () => import('./stock-locations/views/StockLocationManager.View.vue'),
      children: [
        {
          path: '',
          name: 'inventory.locations.list',
          component: () => import('./stock-locations/views/StockLocationList.View.vue'),
          meta: { breadcrumb: 'All Locations' },
        },
        {
          path: 'create',
          name: 'inventory.locations.create',
          component: () => import('./stock-locations/views/StockLocationForm.View.vue'),
          props: { hideHeader: true },
          meta: { breadcrumb: 'Add Location' }
        },
        {
          path: ':id/edit',
          name: 'inventory.locations.edit',
          component: () => import('./stock-locations/views/StockLocationForm.View.vue'),
          props: { hideHeader: true },
          meta: { breadcrumb: 'Edit Location' }
        },
      ]
    },
    {
      path: 'transfers',
      name: 'inventory.transfers.list',
      component: () => import('./stock-transfers/views/StockTransferList.View.vue'),
      meta: { breadcrumb: 'Logistics' }
    },
    {
      path: 'transfers/create',
      name: 'inventory.transfers.create',
      component: () => import('./stock-transfers/views/StockTransferForm.View.vue'),
      meta: { breadcrumb: 'Initiate Transfer' }
    },
    {
      path: 'transfers/:id',
      name: 'inventory.transfers.detail',
      component: () => import('./stock-transfers/views/StockTransferDetail.View.vue'),
      meta: { breadcrumb: 'Transfer Details' }
    }
  ]
}
