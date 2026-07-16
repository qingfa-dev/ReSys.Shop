import { createRouter, createWebHistory } from 'vue-router'
import AppLayout from '@/app/layout/main.layout.vue'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { errorRoutes } from '@/features/error/error.routes'
import { catalogRoutes } from '@/features/catalog/catalog.routes'
import { reportsRoutes } from '@/features/reports/reports.routes'
import { inventoryRoutes } from '@/features/inventories/inventory.routes'
import { orderingRoutes } from '@/features/ordering/ordering.routes'
import { usersRoutes } from '@/features/users/users.routes'
import { rolesRoutes } from '@/features/users/roles.routes'
import { permissionsRoutes } from '@/features/users/permissions.routes'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...errorRoutes,
    rolesRoutes,
    permissionsRoutes,
    { path: '/login', name: 'login', component: () => import('@/features/auth/views/login.view.vue'), meta: { public: true } },
    { path: '/', component: AppLayout, meta: { breadcrumb: 'navigation.home' }, children: [
      { path: '', name: 'home', redirect: { name: 'reports.dashboard' } },
      { path: 'profile', name: 'profile', component: () => import('@/features/auth/views/Profile.view.vue'), meta: { breadcrumb: 'My Profile' } },
      catalogRoutes,
      reportsRoutes,
      inventoryRoutes,
      orderingRoutes,
      usersRoutes,
    ]},
  ],
})

router.beforeEach((to) => {
  const authStore = useAuthStore()
  if (!to.meta.public && !authStore.isAuthenticated) return '/login'
  if (to.path === '/login' && authStore.isAuthenticated) return '/'
})

export default router
