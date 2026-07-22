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
      path: 'orders/new',
      name: 'ordering.orders.create',
      component: () => import('@/features/ordering/pages/OrderDetailPage.vue'),
    },
    {
      path: 'orders/:id',
      name: 'ordering.orders.view',
      component: () => import('@/features/ordering/pages/OrderDetailPage.vue'),
    },
    {
      path: 'orders/:id/edit',
      name: 'ordering.orders.edit',
      component: () => import('@/features/ordering/pages/OrderDetailPage.vue'),
    },
    {
      path: 'fulfillment',
      name: 'ordering.fulfillment.queue',
      component: () => import('@/features/ordering/pages/FulfillmentQueuePage.vue'),
    },
  ],
}
