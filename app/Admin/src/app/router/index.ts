import { createRouter, createWebHistory } from 'vue-router'
import AppLayout from '@/app/layout/AppLayout.vue'
import { useAuthStore } from '@/features/auth/store/auth.store'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/features/auth/pages/Login.Page.vue'),
      meta: { public: true },
    },
    {
      path: '/',
      component: AppLayout,
      children: [
        {
          path: '',
          name: 'dashboard',
          component: () => import('@/features/dashboard/pages/Dashboard.Page.vue'),
        },
      ],
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/features/error/pages/NotFound.Page.vue'),
      meta: { public: true },
    },
  ],
})

router.beforeEach((to) => {
  const authStore = useAuthStore()
  if (!to.meta.public && !authStore.isAuthenticated) return '/login'
  if (to.path === '/login' && authStore.isAuthenticated) return '/'
})

export default router
