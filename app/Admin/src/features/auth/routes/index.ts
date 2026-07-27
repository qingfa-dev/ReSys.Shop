import type { RouteRecordRaw } from 'vue-router'

const LoginPage = () => import('../views/LoginPage.vue')

export const authRoutes: RouteRecordRaw[] = [
  {
    path: 'login',
    name: 'login',
    component: LoginPage,
    meta: { title: 'Sign In', requiresAuth: false },
  },
]
