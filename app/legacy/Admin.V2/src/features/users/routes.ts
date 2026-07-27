import type { RouteRecordRaw } from 'vue-router'

const ROUTE = {
  STAFF: { LIST: 'users.staff.list', CREATE: 'users.staff.create', VIEW: 'users.staff.view', EDIT: 'users.staff.edit' },
  CUSTOMERS: { LIST: 'users.customers.list', CREATE: 'users.customers.create', VIEW: 'users.customers.view', EDIT: 'users.customers.edit' },
  ROLES: { LIST: 'users.roles.list', CREATE: 'users.roles.create', VIEW: 'users.roles.view', EDIT: 'users.roles.edit' },
  PERMISSIONS: { LIST: 'users.permissions.list', VIEW: 'users.permissions.view' },
} as const

export { ROUTE }

export const usersRoutes: RouteRecordRaw = {
  path: 'users',
  children: [
    { path: '', redirect: { name: 'users.staff.list' } },
    {
      path: 'staff',
      name: ROUTE.STAFF.LIST,
      component: () => import('@/features/users/pages/StaffListPage.vue'),
    },
    {
      path: 'staff/new',
      name: ROUTE.STAFF.CREATE,
      component: () => import('@/features/users/pages/StaffDetailPage.vue'),
    },
    {
      path: 'staff/:id',
      name: ROUTE.STAFF.VIEW,
      component: () => import('@/features/users/pages/StaffDetailPage.vue'),
    },
    {
      path: 'staff/:id/edit',
      name: ROUTE.STAFF.EDIT,
      component: () => import('@/features/users/pages/StaffDetailPage.vue'),
    },
    {
      path: 'customers',
      name: ROUTE.CUSTOMERS.LIST,
      component: () => import('@/features/users/pages/CustomerListPage.vue'),
    },
    {
      path: 'customers/new',
      name: ROUTE.CUSTOMERS.CREATE,
      component: () => import('@/features/users/pages/CustomerDetailPage.vue'),
    },
    {
      path: 'customers/:id',
      name: ROUTE.CUSTOMERS.VIEW,
      component: () => import('@/features/users/pages/CustomerDetailPage.vue'),
    },
    {
      path: 'customers/:id/edit',
      name: ROUTE.CUSTOMERS.EDIT,
      component: () => import('@/features/users/pages/CustomerDetailPage.vue'),
    },
    {
      path: 'roles',
      name: ROUTE.ROLES.LIST,
      component: () => import('@/features/users/pages/RoleListPage.vue'),
    },
    {
      path: 'roles/new',
      name: ROUTE.ROLES.CREATE,
      component: () => import('@/features/users/pages/RoleDetailPage.vue'),
    },
    {
      path: 'roles/:id',
      name: ROUTE.ROLES.VIEW,
      component: () => import('@/features/users/pages/RoleDetailPage.vue'),
    },
    {
      path: 'roles/:id/edit',
      name: ROUTE.ROLES.EDIT,
      component: () => import('@/features/users/pages/RoleDetailPage.vue'),
    },
    {
      path: 'permissions',
      name: ROUTE.PERMISSIONS.LIST,
      component: () => import('@/features/users/pages/PermissionListPage.vue'),
    },
    {
      path: 'permissions/:id',
      name: ROUTE.PERMISSIONS.VIEW,
      component: () => import('@/features/users/pages/PermissionDetailPage.vue'),
    },
  ],
}
