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
      component: () => import('./views/StockItemList.View.vue'),
      meta: { breadcrumb: 'Stock Levels' }
    },
    {
      path: 'units',
      name: 'inventory.units.list',
      component: () => import('./views/InventoryUnitList.View.vue'),
      meta: { breadcrumb: 'Serialized Units' }
    },
    {
      path: 'locations',
      meta: { breadcrumb: 'Warehouses' },
      component: () => import('./views/StockLocationManager.View.vue'),
      children: [
        {
          path: '',
          name: 'inventory.locations.list',
          component: () => import('./views/StockLocationList.View.vue'),
        },
        {
          path: 'create',
          name: 'inventory.locations.create',
          component: () => import('./views/StockLocationForm.View.vue'),
          props: { hideHeader: true },
          meta: { breadcrumb: 'Add Location' }
        },
        {
          path: ':id/edit',
          name: 'inventory.locations.edit',
          component: () => import('./views/StockLocationForm.View.vue'),
          props: { hideHeader: true },
          meta: { breadcrumb: 'Edit Location' }
        },
      ]
    },
    {
      path: 'transfers',
      name: 'inventory.transfers.list',
      component: () => import('./views/StockTransferList.View.vue'),
      meta: { breadcrumb: 'Logistics' }
    },
    {
      path: 'transfers/create',
      name: 'inventory.transfers.create',
      component: () => import('./views/StockTransferForm.View.vue'),
      meta: { breadcrumb: 'Initiate Transfer' }
    },
    {
      path: 'transfers/:id',
      name: 'inventory.transfers.detail',
      component: () => import('./views/StockTransferDetail.View.vue'),
      meta: { breadcrumb: 'Transfer Details' }
    }
  ]
}
