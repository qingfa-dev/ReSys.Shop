import type { RouteRecordRaw } from 'vue-router'

const ShippingMethodsList = () => import('../views/ShippingMethodsList.vue')
const ShippingMethodDetail = () => import('../views/ShippingMethodDetail.vue')
const ShippingRatesList = () => import('../views/ShippingRatesList.vue')
const ShippingRateDetail = () => import('../views/ShippingRateDetail.vue')

export const shippingRoutes: RouteRecordRaw[] = [
  {
    path: 'shipping',
    redirect: { name: 'shipping-methods' },
  },
  {
    path: 'shipping/shipping-methods',
    name: 'shipping-methods',
    component: ShippingMethodsList,
    meta: { title: 'Shipping Methods' },
  },
  {
    path: 'shipping/shipping-methods/:id',
    name: 'shipping-method-detail',
    component: ShippingMethodDetail,
    meta: { title: 'Shipping Method Detail' },
  },
  {
    path: 'shipping/shipping-rates',
    name: 'shipping-rates',
    component: ShippingRatesList,
    meta: { title: 'Shipping Rates' },
  },
  {
    path: 'shipping/shipping-rates/:id',
    name: 'shipping-rate-detail',
    component: ShippingRateDetail,
    meta: { title: 'Shipping Rate Detail' },
  },
]

export const shippingMenuItems = [
  {
    label: 'Shipping',
    icon: 'pi pi-fw pi-truck',
    items: [
      { label: 'Methods', icon: 'pi pi-fw pi-cog', to: '/shipping/shipping-methods' },
      { label: 'Rates', icon: 'pi pi-fw pi-ticket', to: '/shipping/shipping-rates' },
    ],
  },
]
