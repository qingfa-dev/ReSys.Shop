import type { RouteRecordRaw } from 'vue-router'

const LoginPage = () => import('../views/LoginPage.vue')
const ForgotPasswordPage = () => import('../views/ForgotPasswordPage.vue')
const ResetPasswordPage = () => import('../views/ResetPasswordPage.vue')

export const authRoutes: RouteRecordRaw[] = [
  {
    path: 'login',
    name: 'login',
    component: LoginPage,
    meta: { title: 'Sign In', subtitle: 'Welcome to ReSys.Shop', requiresAuth: false, guestOnly: true },
  },
  {
    path: 'forgot-password',
    name: 'forgot-password',
    component: ForgotPasswordPage,
    meta: { title: 'Forgot Password', subtitle: 'Enter your email to reset your password', requiresAuth: false, guestOnly: true },
  },
  {
    path: 'reset-password',
    name: 'reset-password',
    component: ResetPasswordPage,
    meta: { title: 'Set New Password', subtitle: 'Choose a new password for your account', requiresAuth: false, guestOnly: true },
  },
]

export const authMenuItems: Array<Record<string, unknown>> = []
