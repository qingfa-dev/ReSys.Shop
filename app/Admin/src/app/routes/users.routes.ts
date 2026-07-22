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
      path: 'staff/new',
      name: 'users.staff.create',
      component: () => import('@/features/users/pages/StaffDetailPage.vue'),
    },
    {
      path: 'staff/:id',
      name: 'users.staff.view',
      component: () => import('@/features/users/pages/StaffDetailPage.vue'),
    },
    {
      path: 'staff/:id/edit',
      name: 'users.staff.edit',
      component: () => import('@/features/users/pages/StaffDetailPage.vue'),
    },
    {
      path: 'customers',
      name: 'users.customers.list',
      component: () => import('@/features/users/pages/CustomerListPage.vue'),
    },
    {
      path: 'customers/new',
      name: 'users.customers.create',
      component: () => import('@/features/users/pages/CustomerDetailPage.vue'),
    },
    {
      path: 'customers/:id',
      name: 'users.customers.view',
      component: () => import('@/features/users/pages/CustomerDetailPage.vue'),
    },
    {
      path: 'customers/:id/edit',
      name: 'users.customers.edit',
      component: () => import('@/features/users/pages/CustomerDetailPage.vue'),
    },
    {
      path: 'roles',
      name: 'users.roles.list',
      component: () => import('@/features/users/pages/RoleListPage.vue'),
    },
    {
      path: 'roles/new',
      name: 'users.roles.create',
      component: () => import('@/features/users/pages/RoleDetailPage.vue'),
    },
    {
      path: 'roles/:id',
      name: 'users.roles.view',
      component: () => import('@/features/users/pages/RoleDetailPage.vue'),
    },
    {
      path: 'roles/:id/edit',
      name: 'users.roles.edit',
      component: () => import('@/features/users/pages/RoleDetailPage.vue'),
    },
    {
      path: 'permissions',
      name: 'users.permissions.list',
      component: () => import('@/features/users/pages/PermissionListPage.vue'),
    },
    {
      path: 'permissions/:id',
      name: 'users.permissions.view',
      component: () => import('@/features/users/pages/PermissionDetailPage.vue'),
    },
  ],
}
