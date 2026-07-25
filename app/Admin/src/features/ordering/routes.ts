import type { RouteRecordRaw } from 'vue-router'

const ROUTE = {
  ORDERS: { LIST: 'ordering.orders.list', CREATE: 'ordering.orders.create', VIEW: 'ordering.orders.view', EDIT: 'ordering.orders.edit' },
  DASHBOARD: 'ordering.dashboard',
  FULFILLMENT: 'ordering.fulfillment.queue',
} as const

export { ROUTE }

export const orderingRoutes: RouteRecordRaw = {
  path: 'ordering',
  children: [
    { path: '', redirect: { name: 'ordering.dashboard' } },
    {
      path: 'dashboard',
      name: 'ordering.dashboard',
      component: () => import('@/features/ordering/pages/DashboardPage.vue'),
    },
    {
      path: 'orders',
      name: ROUTE.ORDERS.LIST,
      component: () => import('@/features/ordering/pages/OrderListPage.vue'),
    },
    {
      path: 'orders/new',
      name: ROUTE.ORDERS.CREATE,
      component: () => import('@/features/ordering/pages/OrderDetailPage.vue'),
    },
    {
      path: 'orders/:id',
      name: ROUTE.ORDERS.VIEW,
      component: () => import('@/features/ordering/pages/OrderDetailPage.vue'),
    },
    {
      path: 'orders/:id/edit',
      name: ROUTE.ORDERS.EDIT,
      component: () => import('@/features/ordering/pages/OrderDetailPage.vue'),
    },
    {
      path: 'fulfillment',
      name: ROUTE.FULFILLMENT,
      component: () => import('@/features/ordering/pages/FulfillmentQueuePage.vue'),
    },
  ],
}
