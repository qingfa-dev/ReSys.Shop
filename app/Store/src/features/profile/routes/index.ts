import type { RouteRecordRaw } from 'vue-router'

export const profileRoutes: RouteRecordRaw[] = [
  {
    path: '/account/addresses',
    name: 'addresses',
    component: () => import('../views/AddressBookView.vue'),
    meta: { requiresAuth: true, title: 'Addresses' },
  },
  {
    path: '/account/profile',
    name: 'profile',
    component: () => import('../views/ProfileView.vue'),
    meta: { requiresAuth: true, title: 'Profile' },
  },
  {
    path: '/account/wishlists',
    name: 'wishlists',
    component: () => import('../views/WishlistsView.vue'),
    meta: { requiresAuth: true, title: 'Wishlists' },
  },
  {
    path: '/account/notifications',
    name: 'notifications',
    component: () => import('../views/NotificationPrefsView.vue'),
    meta: { requiresAuth: true, title: 'Notifications' },
  },
]
