import type { RouteRecordRaw } from 'vue-router'

export const orderingRoutes: RouteRecordRaw = {
  path: 'ordering',
  meta: { breadcrumb: 'Orders' },
  children: [
    {
      path: '',
      name: 'ordering.dashboard',
      component: () => import('./dashboard/pages/OrderingDashboardPage.vue'),
      meta: { breadcrumb: 'Overview' },
    },
    {
      path: 'orders',
      name: 'ordering.orders.list',
      component: () => import('@/features/ordering/orders/pages/OrderListPage.vue'),
      meta: { breadcrumb: 'All Orders' },
    },
    {
      path: 'orders/create',
      name: 'ordering.orders.create',
      component: () => import('@/features/ordering/orders/pages/OrderFormPage.vue'),
      meta: { breadcrumb: 'Create Order' },
    },
    {
      path: 'orders/:id',
      name: 'ordering.orders.detail',
      component: () => import('@/features/ordering/orders/pages/OrderDetailPage.vue'),
      meta: { breadcrumb: 'Detail' },
    },
    {
      path: 'fulfillment',
      name: 'ordering.fulfillment.queue',
      component: () => import('@/features/ordering/fulfillment/pages/FulfillmentQueuePage.vue'),
      meta: { breadcrumb: 'Fulfillment' },
    },
  ],
}
