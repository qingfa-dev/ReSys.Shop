import type { RouteRecordRaw } from 'vue-router'

const UsersList = () => import('../views/UsersList.vue')
const UserDetail = () => import('../views/UserDetail.vue')
const RolesList = () => import('../views/RolesList.vue')
const RoleDetail = () => import('../views/RoleDetail.vue')
const PermissionsList = () => import('../views/PermissionsList.vue')

export const identityRoutes: RouteRecordRaw[] = [
  {
    path: 'identity',
    redirect: { name: 'identity-users' },
  },
  {
    path: 'identity/users',
    name: 'identity-users',
    component: UsersList,
    meta: { title: 'Users' },
  },
  {
    path: 'identity/users/:id',
    name: 'identity-user-detail',
    component: UserDetail,
    meta: { title: 'User Detail' },
  },
  {
    path: 'identity/roles',
    name: 'identity-roles',
    component: RolesList,
    meta: { title: 'Roles' },
  },
  {
    path: 'identity/roles/:id',
    name: 'identity-role-detail',
    component: RoleDetail,
    meta: { title: 'Role Detail' },
  },
  {
    path: 'identity/permissions',
    name: 'identity-permissions',
    component: PermissionsList,
    meta: { title: 'Permissions' },
  },
]

export const identityMenuItems = [
  {
    label: 'Identity',
    icon: 'pi pi-fw pi-users',
    items: [
      { label: 'Users', icon: 'pi pi-fw pi-user', to: '/identity/users' },
      { label: 'Roles', icon: 'pi pi-fw pi-shield', to: '/identity/roles' },
      { label: 'Permissions', icon: 'pi pi-fw pi-key', to: '/identity/permissions' },
    ],
  },
]
