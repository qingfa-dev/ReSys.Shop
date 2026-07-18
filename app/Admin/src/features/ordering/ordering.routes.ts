import type { RouteRecordRaw } from 'vue-router'

export const orderingRoutes: RouteRecordRaw = {
  path: 'ordering',
  meta: { breadcrumb: 'Orders' },
  children: [
    {
      path: '',
      name: 'ordering.dashboard',
      component: () => import('./dashboard/views/OrderingDashboard.View.vue'),
      meta: { breadcrumb: 'Overview' },
    },
    {
      path: 'orders',
      name: 'ordering.orders.list',
      component: () => import('@/features/ordering/orders/views/OrderList.View.vue'),
    },
    {
      path: 'orders/create',
      name: 'ordering.orders.create',
      component: () => import('@/features/ordering/orders/views/OrderForm.View.vue'),
      meta: { breadcrumb: 'Create Order' },
    },
    {
      path: 'orders/:id',
      name: 'ordering.orders.detail',
      component: () => import('@/features/ordering/orders/views/OrderDetail.View.vue'),
      meta: { breadcrumb: 'Detail' },
    },
    {
      path: 'fulfillment',
      name: 'ordering.fulfillment.queue',
      component: () => import('@/features/ordering/fulfillment/views/FulfillmentQueue.View.vue'),
      meta: { breadcrumb: 'Fulfillment' },
    },
  ],
}
