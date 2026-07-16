import type { RouteRecordRaw } from 'vue-router'

export const reportsRoutes: RouteRecordRaw = {
  path: 'reports',
  meta: { breadcrumb: 'Analytics' },
  children: [
    {
      path: '',
      redirect: { name: 'reports.dashboard' },
    },
    {
      path: 'dashboard',
      name: 'reports.dashboard',
      component: () => import('@/features/reports/views/dashboard.view.vue'),
      meta: { breadcrumb: 'Command Center' },
    },
  ],
}
