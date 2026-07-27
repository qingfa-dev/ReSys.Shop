import type { RouteRecordRaw } from 'vue-router'

export const shippingRoutes: RouteRecordRaw = {
  path: 'shipping',
  meta: { breadcrumb: 'Shipping' },
  children: [
    {
      path: '',
      name: 'shipping.methods.list',
      component: () => import('./shipping-methods/pages/ShippingMethodListPage.vue'),
      meta: { breadcrumb: 'Shipping Methods' },
    },
    {
      path: 'methods/create',
      name: 'shipping.methods.create',
      component: () => import('./shipping-methods/pages/ShippingMethodFormPage.vue'),
      meta: { breadcrumb: 'Add Method' },
    },
    {
      path: 'methods/:id/edit',
      name: 'shipping.methods.edit',
      component: () => import('./shipping-methods/pages/ShippingMethodFormPage.vue'),
      meta: { breadcrumb: 'Edit Method' },
    },
    {
      path: 'rates',
      name: 'shipping.rates.list',
      component: () => import('./shipping-rates/pages/ShippingRateListPage.vue'),
      meta: { breadcrumb: 'Shipping Rates' },
    },
    {
      path: 'rates/create',
      name: 'shipping.rates.create',
      component: () => import('./shipping-rates/pages/ShippingRateFormPage.vue'),
      meta: { breadcrumb: 'Add Rate' },
    },
    {
      path: 'rates/:id/edit',
      name: 'shipping.rates.edit',
      component: () => import('./shipping-rates/pages/ShippingRateFormPage.vue'),
      meta: { breadcrumb: 'Edit Rate' },
    },
  ],
}
