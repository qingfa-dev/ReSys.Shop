import { createRouter, createWebHistory } from 'vue-router'
import AppLayout from '../layout/main.layout.vue'
import { useAuthStore } from '@/features/auth/stores/auth.store'

// Feature Routes
import { catalogRoutes } from '@/features/catalog/catalog.routes'
import { inventoryRoutes } from '@/features/inventories/inventory.routes'
import { orderingRoutes } from '@/features/ordering/ordering.routes'
import { usersRoutes } from '@/features/users/users.routes'
import { rolesRoutes } from '@/features/users/roles.routes'
import { permissionsRoutes } from '@/features/users/permissions.routes'
import { reportsRoutes } from '@/features/reports/reports.routes'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/features/auth/views/login.view.vue'),
      meta: { public: true },
    },
    {
      path: '/',
      component: AppLayout,
      meta: { breadcrumb: 'navigation.home' },
      children: [
        {
          path: '',
          name: 'home',
          redirect: { name: 'reports.dashboard' },
        },
        {
          path: 'about',
          name: 'about',
          component: () => import('../views/about.view.vue'),
          meta: { breadcrumb: 'navigation.about' },
        },
        {
          path: 'profile',
          name: 'profile',
          component: () => import('@/features/auth/views/Profile.view.vue'),
          meta: { breadcrumb: 'My Profile' },
        },
        // Feature Modules
        catalogRoutes,
        inventoryRoutes,
        orderingRoutes,
        usersRoutes,
        rolesRoutes,
        permissionsRoutes,
        reportsRoutes,
      ],
    },
  ],
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  const isPublic = to.meta.public || false
  const isAuthenticated = authStore.isAuthenticated

  if (!isPublic && !isAuthenticated) {
    next('/login')
  } else if (to.path === '/login' && isAuthenticated) {
    next('/')
  } else {
    next()
  }
})

export default router