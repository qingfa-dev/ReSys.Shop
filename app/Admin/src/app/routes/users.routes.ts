import type { RouteRecordRaw } from 'vue-router'

export const usersRoutes: RouteRecordRaw = {
  path: 'users',
  children: [
    { path: '', redirect: { name: 'users.staff.list' } },
    {
      path: 'staff',
      name: 'users.staff.list',
      component: () => import('@/features/users/pages/StaffListPage.vue'),
    },
    {
      path: 'staff/create',
      name: 'users.staff.create',
      component: () => import('@/features/users/pages/StaffCreatePage.vue'),
    },
    {
      path: 'customers',
      name: 'users.customers.list',
      component: () => import('@/features/users/pages/CustomerListPage.vue'),
    },
    {
      path: 'roles',
      name: 'users.roles.list',
      component: () => import('@/features/users/pages/RoleListPage.vue'),
    },
    {
      path: 'permissions',
      name: 'users.permissions.list',
      component: () => import('@/features/users/pages/PermissionListPage.vue'),
    },
  ],
}
