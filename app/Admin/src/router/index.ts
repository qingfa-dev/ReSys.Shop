import { createRouter, createWebHistory } from 'vue-router'
import MainLayout from '@/app/layout/MainLayout.vue'
import { reportsRoutes } from '@/app/routes/reports.routes'
import { catalogRoutes } from '@/app/routes/catalog.routes'
import { inventoryRoutes } from '@/app/routes/inventory.routes'
import { orderingRoutes } from '@/app/routes/ordering.routes'
import { paymentRoutes } from '@/app/routes/payment.routes'
import { shippingRoutes } from '@/app/routes/shipping.routes'
import { locationRoutes } from '@/app/routes/location.routes'
import { usersRoutes } from '@/app/routes/users.routes'
import { profileRoutes } from '@/app/routes/profile.routes'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: MainLayout,
      children: [
        { path: '', redirect: { name: 'reports.dashboard' } },
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

export default router
