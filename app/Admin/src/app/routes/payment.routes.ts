import type { RouteRecordRaw } from 'vue-router'

export const paymentRoutes: RouteRecordRaw = {
  path: 'payments',
  children: [
    { path: '', redirect: { name: 'payment.payments.list' } },
    {
      path: 'payments',
      name: 'payment.payments.list',
      component: () => import('@/features/payment/pages/PaymentListPage.vue'),
    },
    {
      path: 'payments/:id',
      name: 'payment.payments.view',
      component: () => import('@/features/payment/pages/PaymentDetailPage.vue'),
    },
    {
      path: 'methods',
      name: 'payment.methods.list',
      component: () => import('@/features/payment/pages/PaymentMethodListPage.vue'),
    },
    {
      path: 'methods/new',
      name: 'payment.methods.create',
      component: () => import('@/features/payment/pages/PaymentMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id',
      name: 'payment.methods.view',
      component: () => import('@/features/payment/pages/PaymentMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id/edit',
      name: 'payment.methods.edit',
      component: () => import('@/features/payment/pages/PaymentMethodDetailPage.vue'),
    },
  ],
}
