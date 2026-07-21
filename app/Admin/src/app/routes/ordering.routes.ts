import type { RouteRecordRaw } from 'vue-router'

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
      name: 'ordering.orders.list',
      component: () => import('@/features/ordering/pages/OrderListPage.vue'),
    },
    {
      path: 'orders/create',
      name: 'ordering.orders.create',
      component: () => import('@/features/ordering/pages/OrderCreatePage.vue'),
    },
    {
      path: 'fulfillment',
      name: 'ordering.fulfillment.queue',
      component: () => import('@/features/ordering/pages/FulfillmentQueuePage.vue'),
    },
  ],
}
