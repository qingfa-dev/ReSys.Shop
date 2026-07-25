import type { RouteRecordRaw } from 'vue-router'

export const profileRoutes: RouteRecordRaw = {
  path: 'profile',
  children: [
    {
      path: '',
      name: 'profile.view',
      component: () => import('@/features/profile/pages/ProfilePage.vue'),
    },
    {
      path: 'addresses',
      name: 'profile.addresses',
      component: () => import('@/features/profile/pages/AddressListPage.vue'),
    },
    {
      path: 'addresses/new',
      name: 'profile.addresses.create',
      component: () => import('@/features/profile/pages/AddressDetailPage.vue'),
    },
    {
      path: 'addresses/:id',
      name: 'profile.addresses.view',
      component: () => import('@/features/profile/pages/AddressDetailPage.vue'),
    },
    {
      path: 'addresses/:id/edit',
      name: 'profile.addresses.edit',
      component: () => import('@/features/profile/pages/AddressDetailPage.vue'),
    },
  ],
}
