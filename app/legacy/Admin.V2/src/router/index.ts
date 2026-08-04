import { createRouter, createWebHistory } from 'vue-router'
import MainLayout from '@/app/layout/MainLayout.vue'
import { reportsRoutes } from '@/features/reports'
import { catalogRoutes } from '@/features/catalog'
import { inventoryRoutes } from '@/features/inventory'
import { orderingRoutes } from '@/features/ordering'
import { paymentRoutes } from '@/features/payment'
import { shippingRoutes } from '@/features/shipping'
import { locationRoutes } from '@/features/location'
import { usersRoutes } from '@/features/users'
import { profileRoutes } from '@/features/profile'
import { authRoutes, changePasswordRoute } from '@/features/auth'
import { registerAuthGuard } from './guards'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...authRoutes,
    {
      path: '/',
      component: MainLayout,
      children: [
        { path: '', redirect: { name: 'reports.dashboard' } },
        changePasswordRoute,
        profileRoutes,
        reportsRoutes,
        catalogRoutes,
        inventoryRoutes,
        orderingRoutes,
        paymentRoutes,
        shippingRoutes,
        locationRoutes,
        usersRoutes,
      ],
    },
  ],
})

registerAuthGuard(router)

export default router
