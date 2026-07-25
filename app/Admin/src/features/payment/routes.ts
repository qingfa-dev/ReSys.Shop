import type { RouteRecordRaw } from 'vue-router'

const ROUTE = {
  PAYMENTS: { LIST: 'payment.payments.list', VIEW: 'payment.payments.view' },
  METHODS: { LIST: 'payment.methods.list', CREATE: 'payment.methods.create', VIEW: 'payment.methods.view', EDIT: 'payment.methods.edit' },
} as const

export { ROUTE }

export const paymentRoutes: RouteRecordRaw = {
  path: 'payments',
  children: [
    { path: '', redirect: { name: ROUTE.PAYMENTS.LIST } },
    {
      path: 'payments',
      name: ROUTE.PAYMENTS.LIST,
      component: () => import('@/features/payment/pages/PaymentListPage.vue'),
    },
    {
      path: 'payments/:id',
      name: ROUTE.PAYMENTS.VIEW,
      component: () => import('@/features/payment/pages/PaymentDetailPage.vue'),
    },
    {
      path: 'methods',
      name: ROUTE.METHODS.LIST,
      component: () => import('@/features/payment/pages/PaymentMethodListPage.vue'),
    },
    {
      path: 'methods/new',
      name: ROUTE.METHODS.CREATE,
      component: () => import('@/features/payment/pages/PaymentMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id',
      name: ROUTE.METHODS.VIEW,
      component: () => import('@/features/payment/pages/PaymentMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id/edit',
      name: ROUTE.METHODS.EDIT,
      component: () => import('@/features/payment/pages/PaymentMethodDetailPage.vue'),
    },
  ],
}
