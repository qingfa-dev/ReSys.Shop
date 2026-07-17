import type { RouteRecordRaw } from 'vue-router'

export const errorRoutes: RouteRecordRaw[] = [
  {
    path: '/error/404',
    name: 'error.404',
    component: () => import('@/features/error/pages/NotFound.view.vue'),
    meta: { public: true },
  },
  {
    path: '/error/500',
    name: 'error.500',
    component: () => import('@/features/error/pages/ErrorPage.view.vue'),
    meta: { public: true },
  },
  {
    path: '/error/403',
    name: 'error.403',
    component: () => import('@/features/error/pages/AccessDenied.view.vue'),
    meta: { public: true },
  },
  {
    path: '/error/empty',
    name: 'error.empty',
    component: () => import('@/features/error/pages/EmptyPage.view.vue'),
    meta: { public: true },
  },
]
