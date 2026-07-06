import type { RouteRecordRaw } from 'vue-router'
import { RouteName } from '@/shared/config/routes'

export const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: RouteName.Login,
    component: () => import('@/features/auth/ui/LoginPage.vue'),
    meta: { authRequired: false, layout: 'blank' },
  },
  {
    path: '/',
    name: RouteName.Dashboard,
    component: () => import('@/features/dashboard/ui/DashboardPage.vue'),
    meta: { authRequired: true },
  },
  {
    path: '/identity/users',
    name: RouteName.Users,
    component: () => import('@/features/identity/users/ui/UserList.vue'),
    meta: { authRequired: true, permission: 'users.read' },
  },
]
