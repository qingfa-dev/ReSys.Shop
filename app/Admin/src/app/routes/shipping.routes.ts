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
      path: 'methods/new',
      name: 'shipping.methods.create',
      component: () => import('@/features/shipping/pages/ShippingMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id',
      name: 'shipping.methods.view',
      component: () => import('@/features/shipping/pages/ShippingMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id/edit',
      name: 'shipping.methods.edit',
      component: () => import('@/features/shipping/pages/ShippingMethodDetailPage.vue'),
    },
    {
      path: 'rates',
      name: 'shipping.rates.list',
      component: () => import('@/features/shipping/pages/ShippingRateListPage.vue'),
    },
    {
      path: 'rates/new',
      name: 'shipping.rates.create',
      component: () => import('@/features/shipping/pages/ShippingRateDetailPage.vue'),
    },
    {
      path: 'rates/:id',
      name: 'shipping.rates.view',
      component: () => import('@/features/shipping/pages/ShippingRateDetailPage.vue'),
    },
    {
      path: 'rates/:id/edit',
      name: 'shipping.rates.edit',
      component: () => import('@/features/shipping/pages/ShippingRateDetailPage.vue'),
    },
  ],
}
