import type { RouteRecordRaw } from 'vue-router'

export const paymentRoutes: RouteRecordRaw = {
  path: 'payments',
  meta: { breadcrumb: 'Payments' },
  children: [
    {
      path: '',
      name: 'payment.payments.list',
      component: () => import('./payments/pages/PaymentListPage.vue'),
      meta: { breadcrumb: 'All Payments' },
    },
    {
      path: ':id',
      name: 'payment.payments.detail',
      component: () => import('./payments/pages/PaymentDetailPage.vue'),
      meta: { breadcrumb: 'Payment Details' },
    },
    {
      path: 'methods',
      name: 'payment.methods.list',
      component: () => import('./payment-methods/pages/PaymentMethodListPage.vue'),
      meta: { breadcrumb: 'Payment Methods' },
    },
    {
      path: 'methods/create',
      name: 'payment.methods.create',
      component: () => import('./payment-methods/pages/PaymentMethodFormPage.vue'),
      meta: { breadcrumb: 'Add Method' },
    },
    {
      path: 'methods/:id/edit',
      name: 'payment.methods.edit',
      component: () => import('./payment-methods/pages/PaymentMethodFormPage.vue'),
      meta: { breadcrumb: 'Edit Method' },
    },
  ],
}
