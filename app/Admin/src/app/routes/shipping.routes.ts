import type { RouteRecordRaw } from 'vue-router'

export const shippingRoutes: RouteRecordRaw = {
  path: 'shipping',
  children: [
    { path: '', redirect: { name: 'shipping.methods.list' } },
    {
      path: 'methods',
      name: 'shipping.methods.list',
      component: () => import('@/features/shipping/pages/ShippingMethodListPage.vue'),
    },
    {
      path: 'rates',
      name: 'shipping.rates.list',
      component: () => import('@/features/shipping/pages/ShippingRateListPage.vue'),
    },
  ],
}
