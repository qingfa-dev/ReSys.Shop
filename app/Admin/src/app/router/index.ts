import { createRouter, createWebHistory } from 'vue-router'
import AppLayout from '@/app/layout/main.layout.vue'
import { useAuthStore } from '@/features/auth/stores/auth.store'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/login', name: 'login', component: () => import('@/features/auth/views/login.view.vue'), meta: { public: true } },
    { path: '/', component: AppLayout, meta: { breadcrumb: 'navigation.home' }, children: [
      { path: '', name: 'home', redirect: { name: 'reports.dashboard' } },
      { path: 'profile', name: 'profile', component: () => import('@/features/profile/views/Profile.view.vue'), meta: { breadcrumb: 'My Profile' } },
    ]},
  ],
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  if (!to.meta.public && !authStore.isAuthenticated) return next('/login')
  if (to.path === '/login' && authStore.isAuthenticated) return next('/')
  next()
})

export default router
