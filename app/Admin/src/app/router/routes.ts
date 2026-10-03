import type { RouteRecordRaw } from 'vue-router'
import { AppLayout } from '@layout'
import { AuthLayout } from '@panel'
import { ErrorPageShell } from '@panel'
import { dashboardRoutes } from '@/features/dashboard/routes'
import { catalogRoutes } from '@/features/catalog/routes'
import { identityRoutes } from '@/features/identity/routes'
import { inventoryRoutes } from '@/features/inventory/routes'
import { locationRoutes } from '@/features/location/routes'
import { orderingRoutes } from '@/features/ordering/routes'
import { paymentRoutes } from '@/features/payment/routes'
import { profileRoutes } from '@/features/profile/routes'
import { shippingRoutes } from '@/features/shipping/routes'
import { authRoutes } from '@/features/auth/routes'

export const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: AppLayout,
    meta: { requiresAuth: true },
    children: [
      ...dashboardRoutes,
      ...catalogRoutes,
      ...identityRoutes,
      ...inventoryRoutes,
      ...locationRoutes,
      ...orderingRoutes,
      ...paymentRoutes,
      ...profileRoutes,
      ...shippingRoutes,
    ],
  },
  {
    path: '/auth',
    component: AuthLayout,
    children: authRoutes,
  },
  {
    path: '/:pathMatch(.*)*',
    component: ErrorPageShell,
    meta: {
      statusCode: 404,
      title: 'Not Found',
      description: 'The page you are looking for does not exist.',
      icon: 'pi pi-search',
    },
  },
]
