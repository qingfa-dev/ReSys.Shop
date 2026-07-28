import type { RouteRecordRaw } from 'vue-router'

const LoginPage = () => import('../views/LoginPage.vue')
const ForgotPasswordPage = () => import('../views/ForgotPasswordPage.vue')
const ResetPasswordPage = () => import('../views/ResetPasswordPage.vue')

export const authRoutes: RouteRecordRaw[] = [
  {
    path: 'login',
    name: 'login',
    component: LoginPage,
    meta: { title: 'Sign In', requiresAuth: false },
  },
  {
    path: 'forgot-password',
    name: 'forgot-password',
    component: ForgotPasswordPage,
    meta: { title: 'Forgot Password', requiresAuth: false },
  },
  {
    path: 'reset-password',
    name: 'reset-password',
    component: ResetPasswordPage,
    meta: { title: 'Reset Password', requiresAuth: false },
  },
]

export const authMenuItems: Array<Record<string, unknown>> = []
