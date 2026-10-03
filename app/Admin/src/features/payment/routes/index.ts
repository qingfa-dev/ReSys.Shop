import type { RouteRecordRaw } from 'vue-router'

const PaymentsList = () => import('../views/PaymentsList.vue')
const PaymentMethodsList = () => import('../views/PaymentMethodsList.vue')
const PaymentMethodDetail = () => import('../views/PaymentMethodDetail.vue')

export const paymentRoutes: RouteRecordRaw[] = [
  {
    path: 'payment',
    redirect: { name: 'payment-payments' },
  },
  {
    path: 'payment/payments',
    name: 'payment-payments',
    component: PaymentsList,
    meta: { title: 'Payments' },
  },
  {
    path: 'payment/payment-methods',
    name: 'payment-methods',
    component: PaymentMethodsList,
    meta: { title: 'Payment Methods' },
  },
  {
    path: 'payment/payment-methods/:id',
    name: 'payment-method-detail',
    component: PaymentMethodDetail,
    meta: { title: 'Payment Method Detail' },
  },
]

export const paymentMenuItems = [
  {
    label: 'Payment',
    icon: 'pi pi-fw pi-credit-card',
    items: [
      { label: 'Payments', icon: 'pi pi-fw pi-dollar', to: '/payment/payments' },
      { label: 'Methods', icon: 'pi pi-fw pi-wallet', to: '/payment/payment-methods' },
    ],
  },
]
