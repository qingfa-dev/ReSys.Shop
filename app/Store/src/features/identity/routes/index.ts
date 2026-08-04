import type { RouteRecordRaw } from 'vue-router'

export const identityRoutes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/LoginView.vue'),
    meta: { guestOnly: true, title: 'Sign In' },
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('../views/RegisterView.vue'),
    meta: { guestOnly: true, title: 'Create Account' },
  },
  {
    path: '/forgot-password',
    name: 'forgot-password',
    component: () => import('../views/ForgotPasswordView.vue'),
    meta: { guestOnly: true, title: 'Forgot Password' },
  },
  {
    path: '/reset-password',
    name: 'reset-password',
    component: () => import('../views/ResetPasswordView.vue'),
    meta: { guestOnly: true, title: 'Reset Password' },
  },
  {
    path: '/account/sessions',
    name: 'sessions',
    component: () => import('../views/SessionsView.vue'),
    meta: { requiresAuth: true, title: 'Sessions' },
  },
]
