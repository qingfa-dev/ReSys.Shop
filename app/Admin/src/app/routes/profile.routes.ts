import type { RouteRecordRaw } from 'vue-router'

export const profileRoutes: RouteRecordRaw = {
  path: 'profile',
  children: [
    {
      path: '',
      name: 'profile',
      component: () => import('@/features/profile/pages/ProfilePage.vue'),
    },
    {
      path: 'addresses',
      name: 'addresses',
      component: () => import('@/features/profile/pages/AddressListPage.vue'),
    },
  ],
}
