import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/features/identity/store/auth'

const publicRoutes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'home',
    component: () => import('@/features/catalog/views/HomeView.vue'),
    meta: { title: 'Home', breadcrumb: 'Home' },
  },
  {
    path: '/shop',
    name: 'shop',
    component: () => import('@/features/catalog/views/ShopView.vue'),
    meta: { title: 'Shop', breadcrumb: 'Shop' },
  },
  {
    path: '/collections',
    name: 'collections',
    component: () => import('@/features/catalog/views/CollectionsView.vue'),
    meta: { title: 'Collections', breadcrumb: 'Collections' },
  },
  {
    path: '/products/:id',
    name: 'product-detail',
    component: () => import('@/features/catalog/views/ProductDetailView.vue'),
    meta: { title: 'Product Details', breadcrumb: 'Product' },
  },

  {
    path: '/login',
    name: 'login',
    component: () => import('@/features/identity/views/LoginView.vue'),
    meta: { title: 'Sign In', guest: true },
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('@/features/identity/views/RegisterView.vue'),
    meta: { title: 'Create Account', guest: true },
  },
  {
    path: '/forgot-password',
    name: 'forgot-password',
    component: () => import('@/features/identity/views/ForgotPasswordView.vue'),
    meta: { title: 'Forgot Password', guest: true },
  },
  {
    path: '/terms',
    name: 'terms',
    component: () => import('@/features/identity/views/TermsView.vue'),
    meta: { title: 'Terms of Service' },
  },
  {
    path: '/privacy',
    name: 'privacy',
    component: () => import('@/features/identity/views/PrivacyView.vue'),
    meta: { title: 'Privacy Policy' },
  },
  {
    path: '/recommendations',
    name: 'recommendations',
    component: () => import('@/features/recommendations/views/RecommendationsView.vue'),
    meta: { title: 'Image Search & Recommendations', breadcrumb: 'Recommendations' },
  },
]

const protectedRoutes: RouteRecordRaw[] = [
  {
    path: '/cart',
    name: 'cart',
    component: () => import('@/features/ordering/views/CartView.vue'),
    meta: { title: 'Shopping Cart', breadcrumb: 'Cart', requiresAuth: true },
  },
  {
    path: '/checkout',
    name: 'checkout',
    component: () => import('@/features/ordering/views/CheckoutView.vue'),
    meta: { title: 'Checkout', breadcrumb: 'Checkout', requiresAuth: true },
  },
  {
    path: '/account',
    component: () => import('@/features/identity/views/AccountView.vue'),
    meta: { title: 'My Account', breadcrumb: 'Account', requiresAuth: true },
    children: [
      {
        path: '',
        redirect: { name: 'orders' },
      },
      {
        path: 'orders',
        name: 'orders',
        component: () => import('@/features/ordering/views/OrdersView.vue'),
        meta: { title: 'My Orders', breadcrumb: 'Orders' },
      },
      {
        path: 'addresses',
        name: 'addresses',
        component: () => import('@/features/locations/views/AddressesView.vue'),
        meta: { title: 'Addresses', breadcrumb: 'Addresses' },
      },
      {
        path: 'profile',
        name: 'profile',
        component: () => import('@/features/profile/views/ProfileView.vue'),
        meta: { title: 'Profile', breadcrumb: 'Profile' },
      },
      {
        path: 'sessions',
        name: 'sessions',
        component: () => import('@/features/identity/views/SessionsView.vue'),
        meta: { title: 'Sessions', breadcrumb: 'Sessions' },
      },
    ],
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...publicRoutes,
    ...protectedRoutes,
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/features/catalog/views/NotFoundView.vue'),
      meta: { title: 'Page Not Found' },
    },
  ],
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    } else {
      return { top: 0 }
    }
  },
})

router.beforeEach(async (to) => {
  document.title = `${to.meta.title || 'ReSys.Shop'} | ReSys.Shop`

  const authStore = useAuthStore()

  if (!authStore.initialized) {
    await authStore.initialize()
  }

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    const redirect = encodeURIComponent(to.fullPath)
    return { name: 'login', query: { redirect } }
  }

  if (to.meta.guest && authStore.isAuthenticated) {
    return { name: 'home' }
  }
})

export default router
