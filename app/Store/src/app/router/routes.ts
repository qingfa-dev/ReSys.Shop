import type { RouteRecordRaw } from 'vue-router'
import { catalogRoutes } from '@/features/catalog/routes'
import { identityRoutes } from '@/features/identity/routes'

export const routes: RouteRecordRaw[] = [
  // Public storefront shell
  {
    path: '/',
    component: () => import('@/app/layouts/DefaultLayout.vue'),
    children: [
      ...catalogRoutes,
      // ordering routes (Phase 4)
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
    children: identityRoutes.filter(r => r.meta?.requiresAuth),
  },
  // 404
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/features/catalog/views/NotFoundView.vue'),
    meta: { title: 'Not Found' },
  },
]
