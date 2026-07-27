import type { RouteRecordRaw } from 'vue-router'

const ROUTE = {
  METHODS: { LIST: 'shipping.methods.list', CREATE: 'shipping.methods.create', VIEW: 'shipping.methods.view', EDIT: 'shipping.methods.edit' },
  RATES: { LIST: 'shipping.rates.list', CREATE: 'shipping.rates.create', VIEW: 'shipping.rates.view', EDIT: 'shipping.rates.edit' },
} as const

export { ROUTE }

export const shippingRoutes: RouteRecordRaw = {
  path: 'shipping',
  children: [
    { path: '', redirect: { name: ROUTE.METHODS.LIST } },
    {
      path: 'methods',
      name: ROUTE.METHODS.LIST,
      component: () => import('@/features/shipping/pages/ShippingMethodListPage.vue'),
    },
    {
      path: 'methods/new',
      name: ROUTE.METHODS.CREATE,
      component: () => import('@/features/shipping/pages/ShippingMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id',
      name: ROUTE.METHODS.VIEW,
      component: () => import('@/features/shipping/pages/ShippingMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id/edit',
      name: ROUTE.METHODS.EDIT,
      component: () => import('@/features/shipping/pages/ShippingMethodDetailPage.vue'),
    },
    {
      path: 'rates',
      name: ROUTE.RATES.LIST,
      component: () => import('@/features/shipping/pages/ShippingRateListPage.vue'),
    },
    {
      path: 'rates/new',
      name: ROUTE.RATES.CREATE,
      component: () => import('@/features/shipping/pages/ShippingRateDetailPage.vue'),
    },
    {
      path: 'rates/:id',
      name: ROUTE.RATES.VIEW,
      component: () => import('@/features/shipping/pages/ShippingRateDetailPage.vue'),
    },
    {
      path: 'rates/:id/edit',
      name: ROUTE.RATES.EDIT,
      component: () => import('@/features/shipping/pages/ShippingRateDetailPage.vue'),
    },
  ],
}
