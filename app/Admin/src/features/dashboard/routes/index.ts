import type { RouteRecordRaw } from 'vue-router'

const DashboardPage = () => import('../views/DashboardPage.vue')

export const dashboardRoutes: RouteRecordRaw[] = [
  {
    path: '',
    name: 'dashboard',
    component: DashboardPage,
    meta: { title: 'Dashboard' },
  },
]

export const dashboardMenuItems = [
  {
    label: 'Dashboard',
    icon: 'pi pi-fw pi-chart-bar',
    to: '/',
  },
]
