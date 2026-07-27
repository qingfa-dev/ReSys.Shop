import type { RouteRecordRaw } from 'vue-router'

export const authRoutes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'auth.login',
    component: () => import('./pages/LoginPage.vue'),
    meta: { layout: 'auth' },
  },
  {
    path: '/register',
    name: 'auth.register',
    component: () => import('./pages/RegisterPage.vue'),
    meta: { layout: 'auth' },
  },
  {
    path: '/forgot-password',
    name: 'auth.forgotPassword',
    component: () => import('./pages/ForgotPasswordPage.vue'),
    meta: { layout: 'auth' },
  },
  {
    path: '/reset-password',
    name: 'auth.resetPassword',
    component: () => import('./pages/ResetPasswordPage.vue'),
    meta: { layout: 'auth' },
  },
]

export const changePasswordRoute: RouteRecordRaw = {
  path: '/account/change-password',
  name: 'auth.changePassword',
  component: () => import('./pages/ChangePasswordPage.vue'),
}
