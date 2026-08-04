import type { RouteRecordRaw } from 'vue-router'

const ProfilesList = () => import('../views/ProfilesList.vue')
const ProfileDetail = () => import('../views/ProfileDetail.vue')
const AddressesList = () => import('../views/AddressesList.vue')
const AddressDetail = () => import('../views/AddressDetail.vue')

export const profileRoutes: RouteRecordRaw[] = [
  {
    path: 'profile/my-profile',
    name: 'profile-my-profile',
    component: () => import('../views/MyProfileView.vue'),
    meta: { title: 'My Profile' },
  },
  {
    path: 'profile',
    redirect: { name: 'profile-profiles' },
  },
  {
    path: 'profile/profiles',
    name: 'profile-profiles',
    component: ProfilesList,
    meta: { title: 'Profiles' },
  },
  {
    path: 'profile/profiles/:id',
    name: 'profile-profile-detail',
    component: ProfileDetail,
    meta: { title: 'Profile Detail' },
  },
  {
    path: 'profile/addresses',
    name: 'profile-addresses',
    component: AddressesList,
    meta: { title: 'Addresses' },
  },
  {
    path: 'profile/addresses/:id',
    name: 'profile-address-detail',
    component: AddressDetail,
    meta: { title: 'Address Detail' },
  },
]

export const profileMenuItems = [
  {
    label: 'Profile',
    icon: 'pi pi-fw pi-id-card',
    items: [
      { label: 'My Profile', icon: 'pi pi-fw pi-user', route: '/profile/my-profile' },
      { label: 'Profiles', icon: 'pi pi-fw pi-user-edit', route: '/profile/profiles' },
      { label: 'Addresses', icon: 'pi pi-fw pi-map-marker', route: '/profile/addresses' },
    ],
  },
]
