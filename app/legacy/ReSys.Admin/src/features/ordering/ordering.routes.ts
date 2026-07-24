import type { RouteRecordRaw } from 'vue-router'

export const orderingRoutes: RouteRecordRaw = {
  path: 'ordering',
  meta: { breadcrumb: 'Orders' },
  children: [
    {
      path: 'orders',
      name: 'ordering.orders.list',
      component: () => import('@/features/ordering/views/order-list.view.vue'),
    },
    {
      path: 'orders/create',
      name: 'ordering.orders.create',
      component: () => import('@/features/ordering/views/order-form.view.vue'),
      meta: { breadcrumb: 'Create Order' },
    },
    {
      path: 'orders/:id',
      name: 'ordering.orders.detail',
      component: () => import('@/features/ordering/views/order-detail.view.vue'),
      meta: { breadcrumb: 'Detail' },
    },
    {
      path: 'fulfillment',
      name: 'ordering.fulfillment.queue',
      component: () => import('@/features/ordering/fulfillment/views/fulfillment-queue.view.vue'),
      meta: { breadcrumb: 'Fulfillment' },
    },
  ],
}
