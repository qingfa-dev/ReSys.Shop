import type { RouteRecordRaw } from 'vue-router'

export const paymentRoutes: RouteRecordRaw = {
  path: 'payments',
  children: [
    { path: '', redirect: { name: 'payment.payments.list' } },
    {
      path: 'list',
      name: 'payment.payments.list',
      component: () => import('@/features/payment/pages/PaymentListPage.vue'),
    },
    {
      path: 'methods',
      name: 'payment.methods.list',
      component: () => import('@/features/payment/pages/PaymentMethodListPage.vue'),
    },
  ],
}
