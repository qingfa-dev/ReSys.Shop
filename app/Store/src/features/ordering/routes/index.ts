import type { RouteRecordRaw } from 'vue-router'
export const orderingRoutes: RouteRecordRaw[] = [
  { path: '/cart', name: 'cart', component: () => import('../views/CartView.vue'), meta: { title: 'Cart' } },
  { path: '/checkout', name: 'checkout', component: () => import('../views/CheckoutView.vue'), meta: { requiresAuth: true, title: 'Checkout' } },
  { path: '/account/orders', name: 'orders', component: () => import('../views/OrderListView.vue'), meta: { requiresAuth: true, title: 'Orders' } },
  { path: '/account/orders/:id', name: 'order-detail', component: () => import('../views/OrderDetailView.vue'), meta: { requiresAuth: true, title: 'Order' } },
]
