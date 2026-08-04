import type { RouteRecordRaw } from 'vue-router'
import { catalogRoutes } from '@/features/catalog/routes'
import { identityRoutes } from '@/features/identity/routes'
import { orderingRoutes } from '@/features/ordering/routes'

export const routes: RouteRecordRaw[] = [
  // Public storefront shell
  {
    path: '/',
    component: () => import('@/app/layouts/DefaultLayout.vue'),
    children: [
      ...catalogRoutes,
      // Ordering routes rendered in the default shell: /cart, /checkout
      ...orderingRoutes.filter(r => !r.path.startsWith('/account')),
    ],
  },
  // Auth pages
  {
    path: '/',
    component: () => import('@/app/layouts/AuthLayout.vue'),
    children: identityRoutes.filter(r => r.meta?.guestOnly),
  },
  // Account pages
  {
    path: '/account',
    component: () => import('@/app/layouts/AccountLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      ...identityRoutes.filter(r => r.meta?.requiresAuth),
      // Ordering routes rendered in the account shell: /account/orders, /account/orders/:id
      ...orderingRoutes.filter(r => r.path.startsWith('/account')),
    ],
  },
  // 404
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/features/catalog/views/NotFoundView.vue'),
    meta: { title: 'Not Found' },
  },
]
