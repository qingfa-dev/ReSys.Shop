import type { RouteRecordRaw } from 'vue-router'

// Context: Catalog feature routes — lazy-loaded for code-splitting per route
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
    path: '/products/:id',
    name: 'product-detail',
    component: () => import('../views/ProductDetailView.vue'),
    meta: { title: 'Product' },
  },
  {
    path: '/recommendations',
    name: 'visual-search',
    component: () => import('../views/VisualSearchView.vue'),
    meta: { title: 'Visual Search' },
  },
  {
    path: '/terms',
    name: 'terms',
    component: () => import('../views/TermsView.vue'),
    meta: { title: 'Terms of Service' },
  },
  {
    path: '/about',
    name: 'about',
    component: () => import('../views/AboutView.vue'),
    meta: { title: 'About Us' },
  },
  {
    path: '/privacy',
    name: 'privacy',
    component: () => import('../views/PrivacyView.vue'),
    meta: { title: 'Privacy Policy' },
  },
]
