import type { RouteRecordRaw } from 'vue-router'

// Context: Account sub-routes — all require authenticated session
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
  {
    path: '/account/change-password',
    name: 'change-password',
    component: () => import('../views/ChangePasswordView.vue'),
    meta: { requiresAuth: true, title: 'Change Password' },
  },
  {
    path: '/account/preferences',
    name: 'preferences',
    component: () => import('../views/PreferencesView.vue'),
    meta: { requiresAuth: true, title: 'Preferences' },
  },
]
