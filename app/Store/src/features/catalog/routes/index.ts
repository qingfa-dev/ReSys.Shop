import type { RouteRecordRaw } from 'vue-router'

export const catalogRoutes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'home',
    component: () => import('../views/HomeView.vue'),
    meta: { title: 'Home' },
  },
  {
    path: '/shop',
    name: 'shop',
    component: () => import('../views/ShopView.vue'),
    meta: { title: 'Shop' },
  },
  {
    path: '/collections',
    name: 'collections',
    component: () => import('../views/CollectionsView.vue'),
    meta: { title: 'Collections' },
  },
  {
    path: '/products/:slug',
    name: 'product-detail',
    component: () => import('../views/ProductDetailView.vue'),
    meta: { title: 'Product' },
  },
  // TODO(task-3.3): add `/recommendations` visual-search route with VisualSearchView
]
