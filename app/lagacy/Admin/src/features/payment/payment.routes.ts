import type { RouteRecordRaw } from 'vue-router'

export const paymentRoutes: RouteRecordRaw = {
  path: 'payments',
  meta: { breadcrumb: 'Payments' },
  children: [
    {
      path: '',
      name: 'payment.payments.list',
      component: () => import('./payments/views/PaymentList.View.vue'),
      meta: { breadcrumb: 'All Payments' },
    },
    {
      path: ':id',
      name: 'payment.payments.detail',
      component: () => import('./payments/views/PaymentDetail.View.vue'),
      meta: { breadcrumb: 'Payment Details' },
    },
    {
      path: 'methods',
      name: 'payment.methods.list',
      component: () => import('./payment-methods/views/PaymentMethodList.View.vue'),
      meta: { breadcrumb: 'Payment Methods' },
    },
    {
      path: 'methods/create',
      name: 'payment.methods.create',
      component: () => import('./payment-methods/views/PaymentMethodForm.View.vue'),
      meta: { breadcrumb: 'Add Method' },
    },
    {
      path: 'methods/:id/edit',
      name: 'payment.methods.edit',
      component: () => import('./payment-methods/views/PaymentMethodForm.View.vue'),
      meta: { breadcrumb: 'Edit Method' },
    },
  ],
}
