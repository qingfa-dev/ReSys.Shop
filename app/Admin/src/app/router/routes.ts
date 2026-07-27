import type { RouteRecordRaw } from 'vue-router'
import { AdminLayout, AuthLayout, ErrorLayout } from '@/app/layouts'

const Placeholder = () => import('@/pages/Placeholder.vue')

export const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: AdminLayout,
    meta: { requiresAuth: true },
    children: [
      { path: '', name: 'dashboard', component: Placeholder, meta: { title: 'Dashboard' } },
      { path: 'catalog/products', name: 'catalog-products', component: Placeholder, meta: { title: 'Products' } },
      { path: 'catalog/categories', name: 'catalog-categories', component: Placeholder, meta: { title: 'Categories' } },
      { path: 'identity/users', name: 'identity-users', component: Placeholder, meta: { title: 'Users' } },
      { path: 'identity/roles', name: 'identity-roles', component: Placeholder, meta: { title: 'Roles' } },
      { path: 'orders', name: 'orders', component: Placeholder, meta: { title: 'Orders' } },
      { path: 'inventory', name: 'inventory', component: Placeholder, meta: { title: 'Inventory' } },
      { path: 'locations', name: 'locations', component: Placeholder, meta: { title: 'Locations' } },
      { path: 'payments', name: 'payments', component: Placeholder, meta: { title: 'Payments' } },
      { path: 'shipping', name: 'shipping', component: Placeholder, meta: { title: 'Shipping' } },
      { path: 'profile', name: 'profile', component: Placeholder, meta: { title: 'Profile & Settings' } },
    ],
  },
  {
    path: '/auth',
    component: AuthLayout,
    props: { title: 'Sign In', subtitle: 'Welcome to ReSys.Shop Admin' },
    children: [
      { path: 'login', name: 'login', component: Placeholder, meta: { title: 'Sign In' } },
    ],
  },
  {
    path: '/:pathMatch(.*)*',
    component: ErrorLayout,
    meta: {
      statusCode: 404,
      title: 'Not Found',
      description: 'The page you are looking for does not exist.',
      icon: 'pi pi-search',
    },
  },
]
