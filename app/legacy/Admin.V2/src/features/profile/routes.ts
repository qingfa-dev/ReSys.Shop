import type { RouteRecordRaw } from 'vue-router'

const ROUTE = {
  PROFILE: { VIEW: 'profile.view' },
  ADDRESSES: { LIST: 'profile.addresses', CREATE: 'profile.addresses.create', VIEW: 'profile.addresses.view', EDIT: 'profile.addresses.edit' },
} as const

export { ROUTE }

export const profileRoutes: RouteRecordRaw = {
  path: 'profile',
  children: [
    {
      path: '',
      name: ROUTE.PROFILE.VIEW,
      component: () => import('@/features/profile/pages/ProfilePage.vue'),
    },
    {
      path: 'addresses',
      name: ROUTE.ADDRESSES.LIST,
      component: () => import('@/features/profile/pages/AddressListPage.vue'),
    },
    {
      path: 'addresses/new',
      name: ROUTE.ADDRESSES.CREATE,
      component: () => import('@/features/profile/pages/AddressDetailPage.vue'),
    },
    {
      path: 'addresses/:id',
      name: ROUTE.ADDRESSES.VIEW,
      component: () => import('@/features/profile/pages/AddressDetailPage.vue'),
    },
    {
      path: 'addresses/:id/edit',
      name: ROUTE.ADDRESSES.EDIT,
      component: () => import('@/features/profile/pages/AddressDetailPage.vue'),
    },
  ],
}
