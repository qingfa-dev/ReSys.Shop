import type { RouteRecordRaw } from 'vue-router'

const ProfilesList = () => import('../views/ProfilesList.vue')
const ProfileDetail = () => import('../views/ProfileDetail.vue')
const AddressesList = () => import('../views/AddressesList.vue')
const AddressDetail = () => import('../views/AddressDetail.vue')

export const profileRoutes: RouteRecordRaw[] = [
  {
    path: 'customer',
    redirect: { name: 'profile-profiles' },
  },
  {
    path: 'customer/profiles',
    name: 'profile-profiles',
    component: ProfilesList,
    meta: { title: 'Profiles' },
  },
  {
    path: 'customer/profiles/:id',
    name: 'profile-profile-detail',
    component: ProfileDetail,
    meta: { title: 'Profile Detail' },
  },
  {
    path: 'customer/addresses',
    name: 'profile-addresses',
    component: AddressesList,
    meta: { title: 'Addresses' },
  },
  {
    path: 'customer/addresses/:id',
    name: 'profile-address-detail',
    component: AddressDetail,
    meta: { title: 'Address Detail' },
  },
]

export const profileMenuItems = [
  {
    label: 'Customer',
    icon: 'pi pi-fw pi-id-card',
    items: [
      { label: 'Profiles', icon: 'pi pi-fw pi-user-edit', to: '/customer/profiles' },
      { label: 'Addresses', icon: 'pi pi-fw pi-map-marker', to: '/customer/addresses' },
    ],
  },
]
